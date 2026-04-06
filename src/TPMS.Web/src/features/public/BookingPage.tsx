import { useDeferredValue, useEffect, useMemo, useState, useTransition } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useSearchParams } from "react-router-dom";
import { AlertBanner } from "../../components/ui/AlertBanner";
import { BayMap } from "../../components/ui/BayMap";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { EmptyState } from "../../components/ui/EmptyState";
import { LoadingSkeleton } from "../../components/ui/LoadingSkeleton";
import { MetricCard } from "../../components/ui/MetricCard";
import { SectionHeading } from "../../components/ui/SectionHeading";
import { StepIndicator } from "../../components/ui/StepIndicator";
import { StatusBadge } from "../../components/ui/StatusBadge";
import { useToast } from "../../app/contexts/ToastContext";
import {
  createReservation,
  fetchAvailability,
  fetchLots,
  isReservationConflictError,
  type BayAvailabilityDto,
  type LotAvailabilitySummaryDto,
} from "../../lib/api";
import { formatCurrency, formatDateTime, formatDurationHours, formatInputDateTime } from "../../lib/format";
import { normalizeLicensePlate } from "../../lib/plate";
import { queryKeys } from "../../lib/queryKeys";
import { setLastReservationId } from "../../lib/session";

type AvailabilityCriteria = {
  lotId: string;
  startUtc: string;
  endUtc: string;
};

const bookingSteps = [
  { id: "search", label: "Find time", description: "Choose lot and window" },
  { id: "bay", label: "Select bay", description: "Review numbered spaces" },
  { id: "review", label: "Confirm", description: "Vehicle details and payment" },
];

const bookingSchema = z
  .object({
    lotId: z.string().min(1, "Choose a parking lot."),
    startLocal: z.string().min(1, "Choose a start time."),
    endLocal: z.string().min(1, "Choose an end time."),
    touristName: z.string().trim().min(2, "Enter the driver name."),
    touristEmail: z.string().trim().email("Enter a valid email address."),
    licensePlate: z.string().trim().min(2, "Enter a license plate."),
  })
  .superRefine((value, context) => {
    const start = new Date(value.startLocal);
    const end = new Date(value.endLocal);

    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
      context.addIssue({
        code: "custom",
        message: "Use valid travel dates.",
        path: ["endLocal"],
      });
      return;
    }

    if (end <= start) {
      context.addIssue({
        code: "custom",
        message: "The end time must be later than the start time.",
        path: ["endLocal"],
      });
    }
  });

type BookingFormValues = z.infer<typeof bookingSchema>;
type AvailabilityFilter = "all" | "available" | "reserved" | "occupied" | "maintenance";

function defaultTimeRange() {
  const start = new Date();
  start.setMinutes(0, 0, 0);
  start.setHours(start.getHours() + 2);

  const end = new Date(start.getTime() + 3 * 60 * 60 * 1000);

  return {
    startLocal: formatInputDateTime(start),
    endLocal: formatInputDateTime(end),
  };
}

function toUtcIso(value: string): string {
  return new Date(value).toISOString();
}

function getAvailabilityFilterOptions() {
  return [
    { value: "all" as const, label: "All bays" },
    { value: "available" as const, label: "Available" },
    { value: "reserved" as const, label: "Reserved" },
    { value: "occupied" as const, label: "Occupied" },
    { value: "maintenance" as const, label: "Maintenance" },
  ];
}

function matchesAvailabilityFilter(bay: BayAvailabilityDto, filter: AvailabilityFilter) {
  switch (filter) {
    case "available":
      return bay.isAvailable;
    case "reserved":
      return bay.isReserved;
    case "occupied":
      return bay.isOccupied;
    case "maintenance":
      return bay.isUnderMaintenance;
    default:
      return true;
  }
}

export function BookingPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { pushToast } = useToast();
  const [searchParams] = useSearchParams();
  const defaults = useMemo(() => defaultTimeRange(), []);
  const [searchCriteria, setSearchCriteria] = useState<AvailabilityCriteria | null>(null);
  const [selectedBayId, setSelectedBayId] = useState<string | null>(null);
  const [availabilityFilter, setAvailabilityFilter] = useState<AvailabilityFilter>("all");
  const [bayTypeFilter, setBayTypeFilter] = useState("All");
  const [conflictAvailability, setConflictAvailability] = useState<LotAvailabilitySummaryDto | null>(null);
  const [conflictMessage, setConflictMessage] = useState<string | null>(null);
  const [isPendingTransition, startTransition] = useTransition();

  const form = useForm<BookingFormValues>({
    resolver: zodResolver(bookingSchema),
    defaultValues: {
      lotId: "",
      startLocal: defaults.startLocal,
      endLocal: defaults.endLocal,
      touristName: "",
      touristEmail: "",
      licensePlate: "",
    },
  });

  const lotsQuery = useQuery({
    queryKey: queryKeys.lots.all,
    queryFn: fetchLots,
  });

  const watchedLotId = form.watch("lotId");
  const watchedStart = form.watch("startLocal");
  const watchedEnd = form.watch("endLocal");

  const selectedLot = useMemo(
    () => lotsQuery.data?.find((lot) => lot.lotId === watchedLotId) ?? lotsQuery.data?.[0] ?? null,
    [lotsQuery.data, watchedLotId],
  );

  useEffect(() => {
    if (!lotsQuery.data?.length || form.getValues("lotId")) {
      return;
    }

    form.setValue("lotId", lotsQuery.data[0].lotId, { shouldDirty: false });
  }, [form, lotsQuery.data]);

  useEffect(() => {
    if (!selectedLot || searchCriteria) {
      return;
    }

    setSearchCriteria({
      lotId: selectedLot.lotId,
      startUtc: toUtcIso(form.getValues("startLocal")),
      endUtc: toUtcIso(form.getValues("endLocal")),
    });
  }, [form, searchCriteria, selectedLot]);

  const availabilityQuery = useQuery({
    queryKey: searchCriteria
      ? queryKeys.availability(searchCriteria.lotId, searchCriteria.startUtc, searchCriteria.endUtc)
      : ["availability", "idle"],
    queryFn: () => fetchAvailability(searchCriteria!.lotId, searchCriteria!.startUtc, searchCriteria!.endUtc),
    enabled: Boolean(searchCriteria),
  });

  const availability = conflictAvailability ?? availabilityQuery.data ?? null;
  const selectedBay = availability?.bays.find((bay) => bay.bayId === selectedBayId) ?? null;

  useEffect(() => {
    if (!selectedBayId || !availability) {
      return;
    }

    const stillSelectable = availability.bays.some((bay) => bay.bayId === selectedBayId && bay.isAvailable);
    if (!stillSelectable) {
      setSelectedBayId(null);
    }
  }, [availability, selectedBayId]);

  const bayTypeOptions = useMemo(() => {
    if (!availability) {
      return ["All"];
    }

    return ["All", ...Array.from(new Set(availability.bays.map((bay) => bay.bayType)))];
  }, [availability]);

  const deferredTypeFilter = useDeferredValue(bayTypeFilter);
  const deferredAvailabilityFilter = useDeferredValue(availabilityFilter);

  const visibleBays = useMemo(() => {
    if (!availability) {
      return [];
    }

    return availability.bays.filter((bay) => {
      const matchesType = deferredTypeFilter === "All" || bay.bayType === deferredTypeFilter;
      return matchesType && matchesAvailabilityFilter(bay, deferredAvailabilityFilter);
    });
  }, [availability, deferredAvailabilityFilter, deferredTypeFilter]);

  const durationHours = useMemo(() => {
    const start = new Date(watchedStart);
    const end = new Date(watchedEnd);
    const differenceMs = end.getTime() - start.getTime();

    if (!Number.isFinite(differenceMs) || differenceMs <= 0) {
      return 0;
    }

    return differenceMs / (60 * 60 * 1000);
  }, [watchedEnd, watchedStart]);

  const estimatedAmount = selectedLot ? selectedLot.hourlyRate * durationHours : 0;

  const reservationMutation = useMutation({
    mutationFn: createReservation,
    onSuccess: (reservation) => {
      setLastReservationId(reservation.reservationId);
      queryClient.setQueryData(queryKeys.reservation(reservation.reservationId), reservation);

      pushToast({
        tone: "success",
        title: "Reservation created",
        description: "Your bay is held. Complete payment in the checkout window to confirm the permit.",
      });

      navigate(`/reservation/${reservation.reservationId}?payment=pending`);

      if (reservation.paymentSession?.checkoutUrl) {
        const popup = window.open(reservation.paymentSession.checkoutUrl, "_blank", "noopener,noreferrer");
        if (!popup) {
          pushToast({
            tone: "warning",
            title: "Checkout blocked",
            description: "Your browser blocked the payment window. Use the reservation page to continue to checkout.",
          });
        }
      }
    },
    onError: (error) => {
      if (isReservationConflictError(error)) {
        setConflictAvailability(error.data?.availability ?? null);
        setConflictMessage(error.data?.error?.message ?? "The selected bay is no longer available.");
        setSelectedBayId(null);

        if (searchCriteria && error.data?.availability) {
          queryClient.setQueryData(queryKeys.availability(searchCriteria.lotId, searchCriteria.startUtc, searchCriteria.endUtc), error.data.availability);
        }

        pushToast({
          tone: "warning",
          title: "Availability changed",
          description: "We refreshed the bay map so you can choose a different bay.",
        });
        return;
      }

      pushToast({
        tone: "danger",
        title: "Reservation failed",
        description: error instanceof Error ? error.message : "We could not create the reservation.",
      });
    },
  });

  const paymentState = searchParams.get("payment");
  const activeStepId = !searchCriteria ? "search" : !selectedBay ? "bay" : "review";

  async function handleSearch() {
    const isValid = await form.trigger(["lotId", "startLocal", "endLocal"]);
    if (!isValid) {
      return;
    }

    const values = form.getValues();
    setConflictAvailability(null);
    setConflictMessage(null);

    startTransition(() => {
      setSelectedBayId(null);
      setSearchCriteria({
        lotId: values.lotId,
        startUtc: toUtcIso(values.startLocal),
        endUtc: toUtcIso(values.endLocal),
      });
    });
  }

  const licensePlateField = form.register("licensePlate");

  return (
    <div className="page-stack">
      <section className="page-header">
        <SectionHeading
          eyebrow="Tourist booking"
          title="Choose the lot, the time window, and the exact bay."
          description="The booking flow keeps availability live, validates your details, and recovers cleanly if another visitor takes the last bay."
        />
        <StepIndicator currentStepId={activeStepId} steps={bookingSteps} />
      </section>

      {paymentState === "cancelled" ? (
        <AlertBanner title="Payment canceled" tone="warning">
          Your bay hold may still be active for a short time. Review the reservation page or start a new booking if needed.
        </AlertBanner>
      ) : null}

      {conflictMessage ? (
        <AlertBanner title="Bay selection changed" tone="warning">
          {conflictMessage}
        </AlertBanner>
      ) : null}

      <form className="page-stack" onSubmit={form.handleSubmit(async (values) => {
        if (!searchCriteria) {
          await handleSearch();
          return;
        }

        if (!selectedBay) {
          pushToast({
            tone: "warning",
            title: "Select a bay first",
            description: "Choose an available numbered bay before continuing to payment.",
          });
          return;
        }

        try {
          await reservationMutation.mutateAsync({
            lotId: values.lotId,
            bayId: selectedBay.bayId,
            touristName: values.touristName.trim(),
            touristEmail: values.touristEmail.trim(),
            isGuestReservation: true,
            licensePlate: normalizeLicensePlate(values.licensePlate),
            startUtc: toUtcIso(values.startLocal),
            endUtc: toUtcIso(values.endLocal),
            successUrl: `${window.location.origin}/reservation/return?payment=success`,
            cancelUrl: `${window.location.origin}/reservation/return?payment=cancelled`,
          });
        } catch {
          return;
        }
      })}>
        <section className="booking-grid">
          <Card className="booking-sidebar">
            <SectionHeading eyebrow="Step 1" title="Travel details" description="Start with the lot and the time window for your visit." />

            <div className="field-stack">
              <label className="field-label">
                <span>Parking lot</span>
                <select {...form.register("lotId")}>
                  {lotsQuery.data?.map((lot) => (
                    <option key={lot.lotId} value={lot.lotId}>
                      {lot.name}
                    </option>
                  ))}
                </select>
                {form.formState.errors.lotId ? <small className="field-error">{form.formState.errors.lotId.message}</small> : null}
              </label>

              <label className="field-label">
                <span>Arrival</span>
                <input type="datetime-local" {...form.register("startLocal")} />
                {form.formState.errors.startLocal ? <small className="field-error">{form.formState.errors.startLocal.message}</small> : null}
              </label>

              <label className="field-label">
                <span>Departure</span>
                <input type="datetime-local" {...form.register("endLocal")} />
                {form.formState.errors.endLocal ? <small className="field-error">{form.formState.errors.endLocal.message}</small> : null}
              </label>
            </div>

            <div className="metric-grid compact-grid">
              <MetricCard label="Estimated stay" value={durationHours > 0 ? formatDurationHours(durationHours) : "-"} detail="Based on your selected time range" />
              <MetricCard
                label="Estimated cost"
                value={selectedLot ? formatCurrency(estimatedAmount, selectedLot.currency) : "-"}
                detail={selectedLot ? `${formatCurrency(selectedLot.hourlyRate, selectedLot.currency)}/hour` : "Choose a lot first"}
                tone="accent"
              />
            </div>

            <Button busy={isPendingTransition} onClick={() => void handleSearch()} tone="secondary" type="button">
              Refresh available bays
            </Button>
          </Card>

          <div className="page-stack">
            <Card>
              <SectionHeading eyebrow="Step 2" title="Bay map" description="Filter by bay type and select an available numbered bay." />

              <div className="legend-row">
                <StatusBadge tone="success">Available</StatusBadge>
                <StatusBadge tone="warning">Reserved</StatusBadge>
                <StatusBadge tone="warning">Occupied</StatusBadge>
                <StatusBadge tone="danger">Maintenance</StatusBadge>
              </div>

              <div className="filter-bar">
                <label className="field-label inline-field">
                  <span>Bay type</span>
                  <select onChange={(event) => setBayTypeFilter(event.target.value)} value={bayTypeFilter}>
                    {bayTypeOptions.map((option) => (
                      <option key={option} value={option}>
                        {option}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="field-label inline-field">
                  <span>Status</span>
                  <select onChange={(event) => setAvailabilityFilter(event.target.value as AvailabilityFilter)} value={availabilityFilter}>
                    {getAvailabilityFilterOptions().map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>
              </div>

              <div className="metric-grid compact-grid">
                <MetricCard label="Available" value={availability?.availableBays ?? "-"} tone="accent" />
                <MetricCard label="Reserved" value={availability?.reservedBays ?? "-"} />
                <MetricCard label="Occupied" value={availability?.occupiedBays ?? "-"} tone="warning" />
              </div>

              {availabilityQuery.isLoading ? <LoadingSkeleton lines={6} /> : null}
              {availabilityQuery.isError ? (
                <EmptyState title="Availability unavailable" description="We could not load the current bay map. Try refreshing the search." />
              ) : null}
              {availability && visibleBays.length === 0 ? (
                <EmptyState title="No bays match these filters" description="Adjust the bay type or status filters to review more options." />
              ) : null}
              {visibleBays.length > 0 ? <BayMap bays={visibleBays} onSelect={(bay) => setSelectedBayId(bay.bayId)} selectedBayId={selectedBayId} /> : null}
            </Card>

            <Card>
              <SectionHeading eyebrow="Step 3" title="Driver details and review" description="Confirm the traveler information before you continue to payment." />

              <div className="field-grid">
                <label className="field-label">
                  <span>Driver name</span>
                  <input {...form.register("touristName")} autoComplete="name" placeholder="Alex Visitor" />
                  {form.formState.errors.touristName ? <small className="field-error">{form.formState.errors.touristName.message}</small> : null}
                </label>

                <label className="field-label">
                  <span>Email</span>
                  <input {...form.register("touristEmail")} autoComplete="email" placeholder="traveler@example.com" type="email" />
                  {form.formState.errors.touristEmail ? <small className="field-error">{form.formState.errors.touristEmail.message}</small> : null}
                </label>
              </div>

              <label className="field-label">
                <span>License plate</span>
                <input
                  {...licensePlateField}
                  autoComplete="off"
                  onBlur={(event) => {
                    licensePlateField.onBlur(event);
                    form.setValue("licensePlate", normalizeLicensePlate(event.target.value), {
                      shouldDirty: true,
                      shouldValidate: true,
                    });
                  }}
                  placeholder="ABC 123"
                />
                {form.formState.errors.licensePlate ? <small className="field-error">{form.formState.errors.licensePlate.message}</small> : null}
              </label>

              <div className="review-card">
                <div>
                  <p className="review-label">Selected bay</p>
                  <strong>{selectedBay ? `${selectedBay.bayNumber} · ${selectedBay.bayType}` : "Choose an available bay"}</strong>
                </div>
                <div>
                  <p className="review-label">Travel window</p>
                  <strong>
                    {searchCriteria ? `${formatDateTime(searchCriteria.startUtc, selectedLot?.timeZoneId)} to ${formatDateTime(searchCriteria.endUtc, selectedLot?.timeZoneId)}` : "Choose a time window"}
                  </strong>
                </div>
                <div>
                  <p className="review-label">Amount due</p>
                  <strong>{selectedLot ? formatCurrency(estimatedAmount, selectedLot.currency) : "-"}</strong>
                </div>
              </div>

              <div className="action-row">
                <Button busy={reservationMutation.isPending} tone="primary" type="submit">
                  Reserve and continue to payment
                </Button>
              </div>
            </Card>
          </div>
        </section>
      </form>
    </div>
  );
}

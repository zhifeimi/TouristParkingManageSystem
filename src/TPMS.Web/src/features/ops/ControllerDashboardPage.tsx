import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { AlertBanner } from "../../components/ui/AlertBanner";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { EmptyState } from "../../components/ui/EmptyState";
import { LoadingSkeleton } from "../../components/ui/LoadingSkeleton";
import { MetricCard } from "../../components/ui/MetricCard";
import { SectionHeading } from "../../components/ui/SectionHeading";
import { StatusBadge } from "../../components/ui/StatusBadge";
import { useConnectionStatus } from "../../app/contexts/ConnectionStatusContext";
import { useToast } from "../../app/contexts/ToastContext";
import {
  createViolation,
  fetchCentralHealth,
  fetchEdgeDashboard,
  fetchEdgeHealth,
  fetchLots,
  validatePermit,
} from "../../lib/api";
import { formatDateTime, formatRelativeTime } from "../../lib/format";
import { normalizeLicensePlate } from "../../lib/plate";
import { queryKeys } from "../../lib/queryKeys";

const permitSchema = z.object({
  licensePlate: z.string().trim().min(2, "Enter a plate to validate."),
});

const violationSchema = z.object({
  lotId: z.string().min(1, "Choose a parking lot."),
  licensePlate: z.string().trim().min(2, "Enter the observed plate."),
  reason: z.string().trim().min(4, "Enter a reason."),
  details: z.string().trim().min(8, "Add enough detail for follow-up."),
});

type PermitFormValues = z.infer<typeof permitSchema>;
type ViolationFormValues = z.infer<typeof violationSchema>;

export function ControllerDashboardPage() {
  const { online, realtimeState } = useConnectionStatus();
  const { pushToast } = useToast();
  const [selectedBayId, setSelectedBayId] = useState<string | null>(null);

  const lotsQuery = useQuery({
    queryKey: queryKeys.lots.all,
    queryFn: fetchLots,
  });

  const dashboardQuery = useQuery({
    queryKey: queryKeys.edgeDashboard,
    queryFn: fetchEdgeDashboard,
    refetchInterval: online ? 15_000 : false,
  });

  const centralHealthQuery = useQuery({
    queryKey: queryKeys.centralHealth,
    queryFn: fetchCentralHealth,
  });

  const edgeHealthQuery = useQuery({
    queryKey: queryKeys.edgeHealth,
    queryFn: fetchEdgeHealth,
  });

  const permitForm = useForm<PermitFormValues>({
    resolver: zodResolver(permitSchema),
    defaultValues: {
      licensePlate: "",
    },
  });

  const violationForm = useForm<ViolationFormValues>({
    resolver: zodResolver(violationSchema),
    defaultValues: {
      lotId: "",
      licensePlate: "",
      reason: "",
      details: "",
    },
  });

  useEffect(() => {
    if (!lotsQuery.data?.length || violationForm.getValues("lotId")) {
      return;
    }

    violationForm.setValue("lotId", lotsQuery.data[0].lotId, { shouldDirty: false });
  }, [lotsQuery.data, violationForm]);

  const selectedBay = dashboardQuery.data?.occupancy.find((bay) => bay.bayId === selectedBayId) ?? null;
  const isStale = dashboardQuery.dataUpdatedAt > 0 && Date.now() - dashboardQuery.dataUpdatedAt > 60_000;

  const permitMutation = useMutation({
    mutationFn: (licensePlate: string) => validatePermit(licensePlate),
    onError: (error) => {
      pushToast({
        tone: "danger",
        title: "Permit lookup failed",
        description: error instanceof Error ? error.message : "The edge node could not validate this permit.",
      });
    },
  });

  const violationMutation = useMutation({
    mutationFn: createViolation,
    onSuccess: () => {
      pushToast({
        tone: "success",
        title: "Violation recorded",
        description: "The enforcement record has been submitted to the central service.",
      });
      violationForm.reset({
        lotId: violationForm.getValues("lotId"),
        licensePlate: "",
        reason: "",
        details: "",
      });
    },
    onError: (error) => {
      pushToast({
        tone: "danger",
        title: "Violation failed",
        description: error instanceof Error ? error.message : "The violation could not be recorded.",
      });
    },
  });

  const healthSummary = useMemo(
    () => ({
      central: centralHealthQuery.data?.status ?? "checking",
      edge: edgeHealthQuery.data?.status ?? "checking",
    }),
    [centralHealthQuery.data?.status, edgeHealthQuery.data?.status],
  );

  return (
    <div className="page-stack">
      <section className="page-header">
        <SectionHeading
          eyebrow="Controller dashboard"
          title="Fast permit decisions, live occupancy, and clearer outage awareness."
          description="The ranger workspace keeps the high-frequency actions close at hand and labels stale or disconnected data instead of failing silently."
        />
      </section>

      {!online ? (
        <AlertBanner title="Working offline" tone="warning">
          The browser is offline. Edge data remains available, but central submissions may queue or fail until connectivity returns.
        </AlertBanner>
      ) : null}

      {edgeHealthQuery.isError ? (
        <AlertBanner title="Edge node unreachable" tone="warning">
          The browser could not reach the edge node health endpoint. Controller data shown here may be incomplete.
        </AlertBanner>
      ) : null}

      {dashboardQuery.isError ? (
        <AlertBanner title="Occupancy board unavailable" tone="warning">
          Live occupancy and sync counts could not be loaded from the edge node.
        </AlertBanner>
      ) : null}

      {dashboardQuery.data?.unsyncedCount ? (
        <AlertBanner title="Pending edge sync" tone="info">
          {dashboardQuery.data.unsyncedCount} local records still need to sync back to the central platform.
        </AlertBanner>
      ) : null}

      {isStale ? (
        <AlertBanner title="Dashboard data is stale" tone="warning">
          The occupancy snapshot has not refreshed recently. Confirm conditions on the lot before taking action.
        </AlertBanner>
      ) : null}

      <section className="metric-grid">
        <MetricCard label="Tracked bays" value={dashboardQuery.isError ? "Unavailable" : dashboardQuery.data?.occupancy.length ?? "-"} tone="accent" />
        <MetricCard label="Recent LPR events" value={dashboardQuery.isError ? "Unavailable" : dashboardQuery.data?.recentLprEvents.length ?? "-"} />
        <MetricCard label="Realtime state" value={realtimeState} detail={`Central ${healthSummary.central} · Edge ${healthSummary.edge}`} />
      </section>

      <section className="ops-dashboard-grid">
        <Card className="ops-primary-card">
          <SectionHeading eyebrow="Live occupancy" title="Bay board" description="Tap a bay to prefill enforcement actions." />
          {dashboardQuery.isLoading ? <LoadingSkeleton lines={8} /> : null}
          {dashboardQuery.isError ? <EmptyState title="Occupancy unavailable" description="The edge node dashboard could not be loaded right now." /> : null}
          {dashboardQuery.data ? (
            <div className="occupancy-board">
              {dashboardQuery.data.occupancy.map((record) => (
                <button
                  key={record.bayId}
                  className={`occupancy-card ${selectedBayId === record.bayId ? "selected" : ""}`}
                  onClick={() => {
                    setSelectedBayId(record.bayId);
                    if (record.licensePlate) {
                      violationForm.setValue("licensePlate", record.licensePlate, { shouldDirty: true });
                    }
                  }}
                  type="button"
                >
                  <div className="occupancy-card-head">
                    <strong>{record.bayNumber}</strong>
                    <StatusBadge tone={record.occupancyStatus.toLowerCase().includes("occupied") ? "warning" : "success"}>
                      {record.occupancyStatus}
                    </StatusBadge>
                  </div>
                  <span>{record.licensePlate ?? "No vehicle detected"}</span>
                  <small>Seen {formatDateTime(record.observedAtUtc)}</small>
                </button>
              ))}
            </div>
          ) : null}
        </Card>

        <div className="page-stack">
          <Card className="sticky-card">
            <SectionHeading eyebrow="Quick permit lookup" title="Validate a permit" />
            <form
              className="field-stack"
              onSubmit={permitForm.handleSubmit(async (values) => {
                await permitMutation.mutateAsync(normalizeLicensePlate(values.licensePlate));
              })}
            >
              <label className="field-label">
                <span>License plate</span>
                <input {...permitForm.register("licensePlate")} placeholder="ABC 123" />
                {permitForm.formState.errors.licensePlate ? <small className="field-error">{permitForm.formState.errors.licensePlate.message}</small> : null}
              </label>
              <Button busy={permitMutation.isPending} tone="secondary" type="submit">
                Validate permit
              </Button>
            </form>

            {permitMutation.data ? (
              <div className="result-card">
                <StatusBadge tone={permitMutation.data.isValid ? "success" : "danger"}>
                  {permitMutation.data.isValid ? "Valid permit" : "No active permit"}
                </StatusBadge>
                <strong>{permitMutation.data.message}</strong>
                <p>
                  {permitMutation.data.bayNumber ? `Assigned bay ${permitMutation.data.bayNumber}` : "No assigned bay in the edge cache."}
                </p>
              </div>
            ) : null}
          </Card>

          <Card>
            <SectionHeading eyebrow="Enforcement action" title="Raise a violation" description="The selected bay is attached automatically when available." />
            <form
              className="field-stack"
              onSubmit={violationForm.handleSubmit(async (values) => {
                await violationMutation.mutateAsync({
                  lotId: values.lotId,
                  bayId: selectedBay?.bayId ?? null,
                  bayNumber: selectedBay?.bayNumber ?? null,
                  licensePlate: normalizeLicensePlate(values.licensePlate),
                  reason: values.reason.trim(),
                  details: values.details.trim(),
                });
              })}
            >
              <label className="field-label">
                <span>Parking lot</span>
                <select {...violationForm.register("lotId")}>
                  {lotsQuery.data?.map((lot) => (
                    <option key={lot.lotId} value={lot.lotId}>
                      {lot.name}
                    </option>
                  ))}
                </select>
                {violationForm.formState.errors.lotId ? <small className="field-error">{violationForm.formState.errors.lotId.message}</small> : null}
              </label>

              <label className="field-label">
                <span>Observed plate</span>
                <input {...violationForm.register("licensePlate")} placeholder="Observed vehicle plate" />
                {violationForm.formState.errors.licensePlate ? <small className="field-error">{violationForm.formState.errors.licensePlate.message}</small> : null}
              </label>

              <label className="field-label">
                <span>Reason</span>
                <input {...violationForm.register("reason")} placeholder="No active permit" />
                {violationForm.formState.errors.reason ? <small className="field-error">{violationForm.formState.errors.reason.message}</small> : null}
              </label>

              <label className="field-label">
                <span>Details</span>
                <textarea {...violationForm.register("details")} placeholder="Describe what the ranger observed." rows={4} />
                {violationForm.formState.errors.details ? <small className="field-error">{violationForm.formState.errors.details.message}</small> : null}
              </label>

              <div className="selected-bay-summary">
                <span>Attached bay</span>
                <strong>{selectedBay ? `${selectedBay.bayNumber} (${selectedBay.licensePlate ?? "No detected plate"})` : "No bay selected"}</strong>
              </div>

              <Button busy={violationMutation.isPending} tone="primary" type="submit">
                Submit violation
              </Button>
            </form>
          </Card>
        </div>
      </section>

      <section className="split-panel">
        <Card>
          <SectionHeading eyebrow="Recent camera activity" title="Latest LPR events" />
          {dashboardQuery.data?.recentLprEvents.length ? (
            <div className="entity-list">
              {dashboardQuery.data.recentLprEvents.map((event) => (
                <article className="entity-row" key={event.id}>
                  <div>
                    <strong>{event.licensePlate}</strong>
                    <p>{event.bayNumber ?? "Bay unknown"}</p>
                  </div>
                  <div className="entity-meta">
                    <StatusBadge tone={event.permitMatched ? "success" : "warning"}>{event.permitMatched ? "Matched" : "Review"}</StatusBadge>
                    <small>{formatDateTime(event.observedAtUtc)}</small>
                  </div>
                </article>
              ))}
            </div>
          ) : (
            <EmptyState title="No recent LPR activity" description="New camera reads will appear here as edge events arrive." />
          )}
        </Card>

        <Card>
          <SectionHeading eyebrow="Operational health" title="Connection summary" />
          <div className="detail-list">
            <div className="detail-row">
              <span>Browser connectivity</span>
              <strong>{online ? "Online" : "Offline"}</strong>
            </div>
            <div className="detail-row">
              <span>Realtime signal</span>
              <strong>{realtimeState}</strong>
            </div>
            <div className="detail-row">
              <span>Central API</span>
              <strong>{centralHealthQuery.data?.status ?? "Checking"}</strong>
            </div>
            <div className="detail-row">
              <span>Edge node</span>
              <strong>{edgeHealthQuery.isError ? "Unavailable" : edgeHealthQuery.data?.status ?? "Checking"}</strong>
            </div>
            <div className="detail-row">
              <span>Last dashboard refresh</span>
              <strong>{dashboardQuery.isError ? "Unavailable" : dashboardQuery.dataUpdatedAt ? formatRelativeTime(dashboardQuery.dataUpdatedAt) : "Waiting"}</strong>
            </div>
          </div>
        </Card>
      </section>
    </div>
  );
}

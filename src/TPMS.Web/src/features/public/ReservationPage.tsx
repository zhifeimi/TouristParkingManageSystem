import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { AlertBanner } from "../../components/ui/AlertBanner";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { EmptyState } from "../../components/ui/EmptyState";
import { LoadingSkeleton } from "../../components/ui/LoadingSkeleton";
import { MetricCard } from "../../components/ui/MetricCard";
import { SectionHeading } from "../../components/ui/SectionHeading";
import { StatusBadge } from "../../components/ui/StatusBadge";
import { fetchReservation } from "../../lib/api";
import { formatCurrency, formatDateTime } from "../../lib/format";
import { queryKeys } from "../../lib/queryKeys";

export function ReservationPage() {
  const navigate = useNavigate();
  const { reservationId } = useParams();
  const [searchParams] = useSearchParams();

  const reservationQuery = useQuery({
    queryKey: reservationId ? queryKeys.reservation(reservationId) : ["reservation", "missing"],
    queryFn: () => fetchReservation(reservationId!),
    enabled: Boolean(reservationId),
  });

  const paymentState = searchParams.get("payment");
  const paymentBanner = useMemo(() => {
    switch (paymentState) {
      case "pending":
        return {
          tone: "info" as const,
          title: "Complete payment",
          description: "Finish checkout in the payment window. If it did not open, use the continue payment button below.",
        };
      case "success":
        return {
          tone: "success" as const,
          title: "Payment return received",
          description: "Your reservation page is refreshed from the central system. Permit confirmation may take a moment to finalize.",
        };
      case "cancelled":
        return {
          tone: "warning" as const,
          title: "Payment canceled",
          description: "You can retry checkout while the reservation hold remains active.",
        };
      default:
        return null;
    }
  }, [paymentState]);

  return (
    <div className="page-stack">
      <section className="page-header">
        <SectionHeading eyebrow="Reservation details" title="Track your bay assignment and payment state." />
      </section>

      {paymentBanner ? (
        <AlertBanner title={paymentBanner.title} tone={paymentBanner.tone}>
          {paymentBanner.description}
        </AlertBanner>
      ) : null}

      {reservationQuery.isLoading ? <LoadingSkeleton lines={6} /> : null}
      {reservationQuery.isError ? (
        <EmptyState title="Reservation unavailable" description="We could not load this reservation. Check the link or start a new booking.">
          <Button onClick={() => navigate("/book")} tone="primary" type="button">
            Start another booking
          </Button>
        </EmptyState>
      ) : null}

      {reservationQuery.data ? (
        <>
          <section className="metric-grid">
            <MetricCard label="Reservation status" value={reservationQuery.data.status} tone="accent" />
            <MetricCard label="Bay number" value={reservationQuery.data.bayNumber} />
            <MetricCard label="Amount" value={formatCurrency(reservationQuery.data.totalAmount, reservationQuery.data.currency)} />
          </section>

          {reservationQuery.data.needsResolution ? (
            <AlertBanner title="Needs controller review" tone="warning">
              {reservationQuery.data.resolutionNote ?? "The reservation needs an updated bay assignment before auto-entry can continue."}
            </AlertBanner>
          ) : null}

          <section className="split-panel">
            <Card>
              <SectionHeading eyebrow="Permit snapshot" title="Visitor information" />
              <div className="detail-list">
                <div className="detail-row">
                  <span>License plate</span>
                  <strong>{reservationQuery.data.licensePlate}</strong>
                </div>
                <div className="detail-row">
                  <span>Arrival</span>
                  <strong>{formatDateTime(reservationQuery.data.startUtc)}</strong>
                </div>
                <div className="detail-row">
                  <span>Departure</span>
                  <strong>{formatDateTime(reservationQuery.data.endUtc)}</strong>
                </div>
                <div className="detail-row">
                  <span>Assignment</span>
                  <strong>{reservationQuery.data.bayNumber}</strong>
                </div>
              </div>
            </Card>

            <Card>
              <SectionHeading eyebrow="Next steps" title="What to do before arrival" />
              <div className="stack-actions">
                <StatusBadge tone={reservationQuery.data.paymentSession?.checkoutUrl ? "info" : "neutral"}>
                  {reservationQuery.data.paymentSession?.checkoutUrl ? "Checkout ready" : "No checkout action available"}
                </StatusBadge>
                {reservationQuery.data.paymentSession?.checkoutUrl ? (
                  <Button onClick={() => window.open(reservationQuery.data.paymentSession!.checkoutUrl, "_blank", "noopener,noreferrer")} tone="primary" type="button">
                    Continue payment
                  </Button>
                ) : null}
                <Button onClick={() => navigate("/book")} tone="secondary" type="button">
                  Book another bay
                </Button>
              </div>
            </Card>
          </section>
        </>
      ) : null}
    </div>
  );
}

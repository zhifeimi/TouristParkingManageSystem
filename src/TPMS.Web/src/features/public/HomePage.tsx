import { useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { MetricCard } from "../../components/ui/MetricCard";
import { Card } from "../../components/ui/Card";
import { EmptyState } from "../../components/ui/EmptyState";
import { LoadingSkeleton } from "../../components/ui/LoadingSkeleton";
import { SectionHeading } from "../../components/ui/SectionHeading";
import { fetchLots } from "../../lib/api";
import { formatCurrency } from "../../lib/format";
import { queryKeys } from "../../lib/queryKeys";
import { getLastReservationId } from "../../lib/session";

export function HomePage() {
  const navigate = useNavigate();
  const lotsQuery = useQuery({
    queryKey: queryKeys.lots.all,
    queryFn: fetchLots,
  });

  const metrics = useMemo(() => {
    const lots = lotsQuery.data ?? [];
    const startingRate = lots.length > 0 ? Math.min(...lots.map((lot) => lot.hourlyRate)) : 0;

    return {
      lotCount: lots.length,
      startingRate,
      currencies: Array.from(new Set(lots.map((lot) => lot.currency))),
    };
  }, [lotsQuery.data]);

  const lastReservationId = getLastReservationId();

  return (
    <div className="page-stack">
      <section className="hero-panel scenic-hero">
        <div className="hero-copy-block">
          <p className="section-eyebrow">Plan your arrival with confidence</p>
          <h1>Reserve a numbered bay before you reach the park gate.</h1>
          <p className="hero-description">
            TPMS helps visitors choose a real parking bay, pay online, and arrive with a permit that still works when connectivity drops across the park.
          </p>
          <div className="hero-actions">
            <button className="button button-primary button-md" onClick={() => navigate("/book")} type="button">
              Start a reservation
            </button>
            {lastReservationId ? (
              <button className="button button-secondary button-md" onClick={() => navigate(`/reservation/${lastReservationId}`)} type="button">
                View latest reservation
              </button>
            ) : null}
          </div>
        </div>
        <Card className="hero-summary-card">
          <SectionHeading eyebrow="Visitor benefits" title="Built for low-connectivity park travel" />
          <ul className="feature-list">
            <li>Choose a specific bay by type, status, and time window.</li>
            <li>Get live occupancy and reservation updates without refreshing.</li>
            <li>Keep ranger enforcement aligned with edge-aware permit validation.</li>
          </ul>
        </Card>
      </section>

      <section className="metric-grid">
        <MetricCard label="Managed lots" value={lotsQuery.data?.length ?? "-"} tone="accent" />
        <MetricCard
          label="Starting hourly rate"
          value={metrics.lotCount > 0 ? formatCurrency(metrics.startingRate, metrics.currencies[0] ?? "USD") : "-"}
          detail="Calculated from current central tariffs"
        />
        <MetricCard label="Booking model" value="Numbered bays" detail="Choose the exact bay you want before checkout" />
      </section>

      <section className="split-panel">
        <Card>
          <SectionHeading eyebrow="How it works" title="A calmer arrival flow" description="The public experience is guided from planning through payment return." />
          <ol className="timeline-list">
            <li>Pick your lot and travel window.</li>
            <li>Review bay availability and choose a numbered bay.</li>
            <li>Confirm your vehicle details and pay online.</li>
            <li>Return to your reservation page for final confirmation and updates.</li>
          </ol>
        </Card>

        <Card>
          <SectionHeading eyebrow="Current lots" title="Available park destinations" />
          {lotsQuery.isLoading ? <LoadingSkeleton lines={5} /> : null}
          {lotsQuery.isError ? <EmptyState title="Lots unavailable" description="We could not load the current parking lots. Please refresh and try again." /> : null}
          {lotsQuery.data ? (
            <div className="entity-list">
              {lotsQuery.data.map((lot) => (
                <article className="entity-row" key={lot.lotId}>
                  <div>
                    <strong>{lot.name}</strong>
                    <p>{lot.code}</p>
                  </div>
                  <span>{formatCurrency(lot.hourlyRate, lot.currency)}/hr</span>
                </article>
              ))}
            </div>
          ) : null}
        </Card>
      </section>
    </div>
  );
}

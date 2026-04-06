import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { AlertBanner } from "../../components/ui/AlertBanner";
import { Card } from "../../components/ui/Card";
import { EmptyState } from "../../components/ui/EmptyState";
import { MetricCard } from "../../components/ui/MetricCard";
import { LoadingSkeleton } from "../../components/ui/LoadingSkeleton";
import { SectionHeading } from "../../components/ui/SectionHeading";
import { StatusBadge } from "../../components/ui/StatusBadge";
import { useConnectionStatus } from "../../app/contexts/ConnectionStatusContext";
import { fetchCentralHealth, fetchEdgeDashboard, fetchEdgeHealth, fetchLots } from "../../lib/api";
import { formatCurrency, formatDateTime } from "../../lib/format";
import { queryKeys } from "../../lib/queryKeys";

export function AdminOverviewPage() {
  const { online, realtimeState } = useConnectionStatus();

  const lotsQuery = useQuery({
    queryKey: queryKeys.lots.all,
    queryFn: fetchLots,
  });

  const dashboardQuery = useQuery({
    queryKey: queryKeys.edgeDashboard,
    queryFn: fetchEdgeDashboard,
  });

  const centralHealthQuery = useQuery({
    queryKey: queryKeys.centralHealth,
    queryFn: fetchCentralHealth,
  });

  const edgeHealthQuery = useQuery({
    queryKey: queryKeys.edgeHealth,
    queryFn: fetchEdgeHealth,
  });

  const startingRate = useMemo(() => {
    if (!lotsQuery.data?.length) {
      return null;
    }

    return Math.min(...lotsQuery.data.map((lot) => lot.hourlyRate));
  }, [lotsQuery.data]);

  return (
    <div className="page-stack">
      <section className="page-header">
        <SectionHeading
          eyebrow="Admin overview"
          title="A clearer operations picture across central and edge systems."
          description="The admin workspace focuses on runtime status, lot inventory, tariff context, and sync readiness without introducing unmanaged features outside the existing API surface."
        />
      </section>

      {!online ? (
        <AlertBanner title="Administrator browser offline" tone="warning">
          Health and lot information may be outdated until browser connectivity is restored.
        </AlertBanner>
      ) : null}

      <section className="metric-grid">
        <MetricCard label="Managed lots" value={lotsQuery.data?.length ?? "-"} tone="accent" />
        <MetricCard
          label="Starting hourly rate"
          value={startingRate !== null && lotsQuery.data?.[0] ? formatCurrency(startingRate, lotsQuery.data[0].currency) : "-"}
          detail="Lowest configured rate across listed lots"
        />
        <MetricCard
          label="Unsynced edge records"
          value={dashboardQuery.isError ? "Unavailable" : dashboardQuery.data?.unsyncedCount ?? "-"}
          detail={dashboardQuery.isError ? "Edge dashboard is unreachable from the browser." : `Realtime ${realtimeState}`}
        />
      </section>

      {edgeHealthQuery.isError ? (
        <AlertBanner title="Edge health unavailable" tone="warning">
          The browser could not reach the edge node health endpoint. Check the edge base URL and CORS settings.
        </AlertBanner>
      ) : null}

      {dashboardQuery.isError ? (
        <AlertBanner title="Edge dashboard unavailable" tone="warning">
          Live occupancy and sync counts are unavailable because the browser could not load the edge controller dashboard.
        </AlertBanner>
      ) : null}

      <section className="split-panel">
        <Card>
          <SectionHeading eyebrow="Runtime health" title="Central and edge status" />
          <div className="entity-list">
            <article className="entity-row">
              <div>
                <strong>{centralHealthQuery.data?.source ?? "Central API"}</strong>
                <p>{centralHealthQuery.data ? `Verified ${formatDateTime(centralHealthQuery.data.checkedAtUtc)}` : "Waiting for status"}</p>
              </div>
              <StatusBadge tone={centralHealthQuery.data?.status === "ok" ? "success" : "warning"}>{centralHealthQuery.data?.status ?? "Checking"}</StatusBadge>
            </article>
            <article className="entity-row">
              <div>
                <strong>{edgeHealthQuery.data?.source ?? "Edge Node"}</strong>
                <p>
                  {edgeHealthQuery.data
                    ? `Verified ${formatDateTime(edgeHealthQuery.data.checkedAtUtc)}`
                    : edgeHealthQuery.isError
                      ? "Browser could not reach the edge node."
                      : "Waiting for status"}
                </p>
              </div>
              <StatusBadge tone={edgeHealthQuery.isError ? "danger" : edgeHealthQuery.data?.status === "ok" ? "success" : "warning"}>
                {edgeHealthQuery.isError ? "Unavailable" : edgeHealthQuery.data?.status ?? "Checking"}
              </StatusBadge>
            </article>
          </div>
        </Card>

        <Card>
          <SectionHeading eyebrow="Environment summary" title="Control-plane observations" />
          <div className="detail-list">
            <div className="detail-row">
              <span>Browser connectivity</span>
              <strong>{online ? "Online" : "Offline"}</strong>
            </div>
            <div className="detail-row">
              <span>Realtime channel</span>
              <strong>{realtimeState}</strong>
            </div>
            <div className="detail-row">
              <span>Unsynced edge items</span>
              <strong>{dashboardQuery.isError ? "Unavailable" : dashboardQuery.data?.unsyncedCount ?? "-"}</strong>
            </div>
            <div className="detail-row">
              <span>Observed occupancy rows</span>
              <strong>{dashboardQuery.isError ? "Unavailable" : dashboardQuery.data?.occupancy.length ?? "-"}</strong>
            </div>
          </div>
        </Card>
      </section>

      <Card>
        <SectionHeading eyebrow="Lot portfolio" title="Parking lots and tariffs" />
        {lotsQuery.isLoading ? <LoadingSkeleton lines={5} /> : null}
        {lotsQuery.isError ? <EmptyState title="Lots unavailable" description="The central lot portfolio could not be loaded right now." /> : null}
        {lotsQuery.data ? (
          <div className="entity-list">
            {lotsQuery.data.map((lot) => (
              <article className="entity-row" key={lot.lotId}>
                <div>
                  <strong>{lot.name}</strong>
                  <p>
                    {lot.code} · {lot.timeZoneId}
                  </p>
                </div>
                <span>{formatCurrency(lot.hourlyRate, lot.currency)}/hr</span>
              </article>
            ))}
          </div>
        ) : null}
      </Card>
    </div>
  );
}

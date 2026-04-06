import { Card } from "./Card";

type Props = {
  label: string;
  value: string | number;
  detail?: string;
  tone?: "neutral" | "accent" | "warning";
};

export function MetricCard({ label, value, detail, tone = "neutral" }: Props) {
  return (
    <Card className={`metric-card metric-${tone}`}>
      <span className="metric-label">{label}</span>
      <strong className="metric-value">{value}</strong>
      {detail ? <span className="metric-detail">{detail}</span> : null}
    </Card>
  );
}


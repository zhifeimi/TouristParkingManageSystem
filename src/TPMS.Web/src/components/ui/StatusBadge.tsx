import type { ReactNode } from "react";

type Props = {
  tone?: "neutral" | "success" | "warning" | "danger" | "info";
  children: ReactNode;
};

export function StatusBadge({ tone = "neutral", children }: Props) {
  return <span className={`status-badge status-${tone}`}>{children}</span>;
}

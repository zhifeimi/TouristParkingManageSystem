import type { PropsWithChildren } from "react";

type Props = PropsWithChildren<{
  tone?: "info" | "success" | "warning" | "danger";
  title: string;
}>;

export function AlertBanner({ tone = "info", title, children }: Props) {
  return (
    <div className={`alert-banner alert-${tone}`} role="status">
      <strong>{title}</strong>
      {children ? <p>{children}</p> : null}
    </div>
  );
}


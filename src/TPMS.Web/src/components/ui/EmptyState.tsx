import type { PropsWithChildren } from "react";

type Props = PropsWithChildren<{
  title: string;
  description: string;
}>;

export function EmptyState({ title, description, children }: Props) {
  return (
    <div className="empty-state">
      <strong>{title}</strong>
      <p>{description}</p>
      {children}
    </div>
  );
}


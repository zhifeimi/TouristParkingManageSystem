import type { HTMLAttributes, PropsWithChildren } from "react";

type CardProps = PropsWithChildren<HTMLAttributes<HTMLElement>>;

export function Card({ children, className = "", ...props }: CardProps) {
  return (
    <section {...props} className={`card ${className}`.trim()}>
      {children}
    </section>
  );
}


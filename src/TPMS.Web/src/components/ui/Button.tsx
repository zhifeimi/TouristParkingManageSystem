import type { ButtonHTMLAttributes, PropsWithChildren } from "react";

type Props = PropsWithChildren<
  ButtonHTMLAttributes<HTMLButtonElement> & {
    tone?: "primary" | "secondary" | "ghost" | "danger";
    size?: "sm" | "md" | "lg";
    fullWidth?: boolean;
    busy?: boolean;
  }
>;

export function Button({
  tone = "primary",
  size = "md",
  fullWidth = false,
  busy = false,
  className = "",
  children,
  disabled,
  ...props
}: Props) {
  return (
    <button
      {...props}
      className={`button button-${tone} button-${size} ${fullWidth ? "button-block" : ""} ${className}`.trim()}
      disabled={disabled || busy}
    >
      {busy ? <span className="button-spinner" aria-hidden="true" /> : null}
      <span>{children}</span>
    </button>
  );
}


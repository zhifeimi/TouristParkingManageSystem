export function formatCurrency(amount: number, currency: string): string {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatDateTime(value: string, timeZone?: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
    timeZone,
  }).format(new Date(value));
}

export function formatRelativeTime(value: number): string {
  const seconds = Math.max(0, Math.round((Date.now() - value) / 1000));

  if (seconds < 60) {
    return "just now";
  }

  if (seconds < 3600) {
    return `${Math.round(seconds / 60)} min ago`;
  }

  return `${Math.round(seconds / 3600)} hr ago`;
}

export function formatDurationHours(hours: number): string {
  if (hours < 1) {
    return `${Math.max(1, Math.round(hours * 60))} min`;
  }

  return `${hours.toFixed(hours % 1 === 0 ? 0 : 1)} hr`;
}

export function formatInputDateTime(value: Date): string {
  const offset = value.getTimezoneOffset();
  const localDate = new Date(value.getTime() - offset * 60_000);
  return localDate.toISOString().slice(0, 16);
}


export function normalizeLicensePlate(value: string): string {
  return value
    .trim()
    .toUpperCase()
    .replace(/[^A-Z0-9\s-]/g, "")
    .replace(/\s+/g, " ");
}


export const queryKeys = {
  lots: {
    all: ["lots"] as const,
  },
  availability: (lotId: string, startUtc: string, endUtc: string) => ["availability", lotId, startUtc, endUtc] as const,
  reservation: (reservationId: string) => ["reservation", reservationId] as const,
  edgeDashboard: ["edge-dashboard"] as const,
  centralHealth: ["central-health"] as const,
  edgeHealth: ["edge-health"] as const,
};


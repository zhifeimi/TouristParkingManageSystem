import { getStoredSession, type SessionUser } from "./session";

export type LotListItemDto = {
  lotId: string;
  code: string;
  name: string;
  timeZoneId: string;
  hourlyRate: number;
  currency: string;
};

export type BayAvailabilityDto = {
  bayId: string;
  bayNumber: string;
  bayType: string;
  isAvailable: boolean;
  isReserved: boolean;
  isOccupied: boolean;
  isUnderMaintenance: boolean;
  occupiedByLicensePlate?: string | null;
};

export type LotAvailabilitySummaryDto = {
  lotId: string;
  lotName: string;
  startUtc: string;
  endUtc: string;
  totalBays: number;
  availableBays: number;
  occupiedBays: number;
  reservedBays: number;
  bays: BayAvailabilityDto[];
};

export type CreatePaymentSessionResponse = {
  sessionId: string;
  checkoutUrl: string;
};

export type ReservationDto = {
  reservationId: string;
  parkingLotId: string;
  parkingBayId: string;
  bayNumber: string;
  status: string;
  startUtc: string;
  endUtc: string;
  licensePlate: string;
  totalAmount: number;
  currency: string;
  needsResolution: boolean;
  resolutionNote?: string | null;
  paymentSession?: CreatePaymentSessionResponse | null;
};

export type DashboardResponse = {
  occupancy: Array<{
    bayId: string;
    bayNumber: string;
    occupancyStatus: string;
    licensePlate?: string | null;
    observedAtUtc: string;
  }>;
  recentLprEvents: Array<{
    id: string;
    licensePlate: string;
    bayNumber?: string | null;
    observedAtUtc: string;
    permitMatched: boolean;
  }>;
  unsyncedCount: number;
};

export type PermitValidationResultDto = {
  licensePlate: string;
  isValid: boolean;
  permitCode?: string | null;
  bayNumber?: string | null;
  validToUtc?: string | null;
  status: string;
  message: string;
};

export type HealthStatusDto = {
  status: string;
  source: string;
  checkedAtUtc: string;
};

export type LoginResponse = {
  token: string;
  user: SessionUser;
};

export type ReservationConflictResponse = {
  error?: {
    code?: string;
    message?: string;
  };
  availability?: LotAvailabilitySummaryDto | null;
};

export type ApiError<TData = unknown> = Error & {
  status: number;
  data?: TData;
};

type CreateReservationPayload = {
  lotId: string;
  bayId: string;
  touristName: string;
  touristEmail: string;
  isGuestReservation: boolean;
  licensePlate: string;
  startUtc: string;
  endUtc: string;
  successUrl?: string;
  cancelUrl?: string;
};

type CreateViolationPayload = {
  lotId: string;
  bayId?: string | null;
  bayNumber?: string | null;
  licensePlate: string;
  reason: string;
  details: string;
};

function buildDefaultOrigin(port: string): string {
  if (typeof window === "undefined") {
    return `http://localhost:${port}`;
  }

  const protocol = window.location.protocol === "https:" ? "https:" : "http:";
  return `${protocol}//${window.location.hostname}:${port}`;
}

function resolveBaseUrl(explicitValue: string | undefined, fallbackPort: string): string {
  if (explicitValue && explicitValue.trim().length > 0) {
    return explicitValue;
  }

  if (typeof window !== "undefined" && window.location.port === fallbackPort) {
    return window.location.origin;
  }

  return buildDefaultOrigin(fallbackPort);
}

const apiBaseUrl = resolveBaseUrl(import.meta.env.VITE_API_BASE_URL, "5080");
const edgeBaseUrl = resolveBaseUrl(import.meta.env.VITE_EDGE_API_BASE_URL, "5081");

export function getCentralBaseUrl(): string {
  return apiBaseUrl;
}

export function getEdgeBaseUrl(): string {
  return edgeBaseUrl;
}

async function parseResponseBody(response: Response): Promise<unknown> {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    return response.json();
  }

  const text = await response.text();
  return text ? { message: text } : null;
}

async function handleResponse<T>(response: Response): Promise<T> {
  const body = await parseResponseBody(response);

  if (!response.ok) {
    const message =
      typeof body === "object" && body !== null && "message" in body && typeof body.message === "string"
        ? body.message
        : response.statusText || "Request failed";

    const error = Object.assign(new Error(message), {
      status: response.status,
      data: body,
    });

    throw error as ApiError;
  }

  return body as T;
}

function authHeaders() {
  const token = getStoredSession()?.token;
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export function isApiError<TData = unknown>(error: unknown): error is ApiError<TData> {
  return error instanceof Error && typeof (error as ApiError).status === "number";
}

export function isReservationConflictError(error: unknown): error is ApiError<ReservationConflictResponse> {
  return isApiError<ReservationConflictResponse>(error) && error.status === 409;
}

export async function fetchLots(): Promise<LotListItemDto[]> {
  const response = await fetch(`${apiBaseUrl}/api/lots`);
  return handleResponse<LotListItemDto[]>(response);
}

export async function fetchAvailability(lotId: string, startUtc: string, endUtc: string): Promise<LotAvailabilitySummaryDto> {
  const query = new URLSearchParams({ startUtc, endUtc });
  const response = await fetch(`${apiBaseUrl}/api/lots/${lotId}/bays/availability?${query.toString()}`);
  return handleResponse<LotAvailabilitySummaryDto>(response);
}

export async function createReservation(payload: CreateReservationPayload): Promise<ReservationDto> {
  const response = await fetch(`${apiBaseUrl}/api/reservations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  return handleResponse<ReservationDto>(response);
}

export async function fetchReservation(reservationId: string): Promise<ReservationDto> {
  const response = await fetch(`${apiBaseUrl}/api/reservations/${reservationId}`);
  return handleResponse<ReservationDto>(response);
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  return handleResponse<LoginResponse>(response);
}

export async function fetchCentralHealth(): Promise<HealthStatusDto> {
  const response = await fetch(`${apiBaseUrl}/api/health`);
  const data = await handleResponse<{ status: string }>(response);

  return {
    status: data.status,
    source: "Central API",
    checkedAtUtc: new Date().toISOString(),
  };
}

export async function fetchEdgeHealth(): Promise<HealthStatusDto> {
  const response = await fetch(`${edgeBaseUrl}/health`);
  const data = await handleResponse<{ status: string }>(response);

  return {
    status: data.status,
    source: "Edge Node",
    checkedAtUtc: new Date().toISOString(),
  };
}

export async function fetchEdgeDashboard(): Promise<DashboardResponse> {
  const response = await fetch(`${edgeBaseUrl}/api/local/controller/dashboard`);
  return handleResponse<DashboardResponse>(response);
}

export async function validatePermit(licensePlate: string): Promise<PermitValidationResultDto> {
  const response = await fetch(`${edgeBaseUrl}/api/local/permits/validate/${encodeURIComponent(licensePlate)}`);
  return handleResponse<PermitValidationResultDto>(response);
}

export async function createViolation(payload: CreateViolationPayload): Promise<unknown> {
  const headers = new Headers({ "Content-Type": "application/json" });
  const tokenHeaders = authHeaders();
  if (tokenHeaders.Authorization) {
    headers.set("Authorization", tokenHeaders.Authorization);
  }

  const response = await fetch(`${apiBaseUrl}/api/enforcement/violations`, {
    method: "POST",
    headers,
    body: JSON.stringify(payload),
  });

  return handleResponse(response);
}

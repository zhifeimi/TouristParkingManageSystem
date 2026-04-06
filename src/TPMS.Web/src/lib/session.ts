export type SessionUser = {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
};

export type StoredSession = {
  token: string;
  user: SessionUser;
};

const sessionStorageKey = "tpms_session";
const lastReservationKey = "tpms_last_reservation_id";

function readJson<T>(key: string): T | null {
  if (typeof window === "undefined" || typeof window.localStorage?.getItem !== "function") {
    return null;
  }

  const value = window.localStorage.getItem(key);
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value) as T;
  } catch {
    return null;
  }
}

export function getStoredSession(): StoredSession | null {
  return readJson<StoredSession>(sessionStorageKey);
}

export function setStoredSession(session: StoredSession): void {
  if (typeof window === "undefined" || typeof window.localStorage?.setItem !== "function") {
    return;
  }

  window.localStorage.setItem(sessionStorageKey, JSON.stringify(session));
}

export function clearStoredSession(): void {
  if (typeof window === "undefined" || typeof window.localStorage?.removeItem !== "function") {
    return;
  }

  window.localStorage.removeItem(sessionStorageKey);
}

export function getLastReservationId(): string | null {
  if (typeof window === "undefined" || typeof window.localStorage?.getItem !== "function") {
    return null;
  }

  return window.localStorage.getItem(lastReservationKey);
}

export function setLastReservationId(reservationId: string): void {
  if (typeof window === "undefined" || typeof window.localStorage?.setItem !== "function") {
    return;
  }

  window.localStorage.setItem(lastReservationKey, reservationId);
}


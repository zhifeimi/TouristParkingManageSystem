import { createContext, useContext, useEffect, useMemo, useState, type PropsWithChildren } from "react";

export type RealtimeState = "offline" | "connecting" | "connected" | "reconnecting" | "disconnected";

type ConnectionStatusContextValue = {
  online: boolean;
  realtimeState: RealtimeState;
  lastRealtimeEventAt: number | null;
  setRealtimeState: (state: RealtimeState) => void;
  markRealtimeEvent: () => void;
};

const ConnectionStatusContext = createContext<ConnectionStatusContextValue | undefined>(undefined);

export function ConnectionStatusProvider({ children }: PropsWithChildren) {
  const [online, setOnline] = useState(() => (typeof navigator === "undefined" ? true : navigator.onLine));
  const [realtimeState, setRealtimeState] = useState<RealtimeState>(online ? "disconnected" : "offline");
  const [lastRealtimeEventAt, setLastRealtimeEventAt] = useState<number | null>(null);

  useEffect(() => {
    function handleOnline() {
      setOnline(true);
      setRealtimeState((currentState) => (currentState === "offline" ? "disconnected" : currentState));
    }

    function handleOffline() {
      setOnline(false);
      setRealtimeState("offline");
    }

    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);

    return () => {
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
    };
  }, []);

  const value = useMemo<ConnectionStatusContextValue>(
    () => ({
      online,
      realtimeState,
      lastRealtimeEventAt,
      setRealtimeState,
      markRealtimeEvent: () => setLastRealtimeEventAt(Date.now()),
    }),
    [lastRealtimeEventAt, online, realtimeState],
  );

  return <ConnectionStatusContext.Provider value={value}>{children}</ConnectionStatusContext.Provider>;
}

export function useConnectionStatus() {
  const context = useContext(ConnectionStatusContext);
  if (!context) {
    throw new Error("useConnectionStatus must be used within ConnectionStatusProvider.");
  }

  return context;
}


import { useEffect, useRef } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../app/contexts/AuthContext";
import { useConnectionStatus } from "../app/contexts/ConnectionStatusContext";
import { getCentralBaseUrl } from "../lib/api";
import { queryKeys } from "../lib/queryKeys";
import { subscribeRealtime } from "../lib/realtime";

export function useRealtimeUpdates() {
  const queryClient = useQueryClient();
  const { session } = useAuth();
  const { online, markRealtimeEvent, setRealtimeState } = useConnectionStatus();
  const stateTimerRef = useRef<number | null>(null);

  useEffect(() => {
    if (import.meta.env.MODE === "test") {
      return undefined;
    }

    function scheduleVisibleState(nextState: "offline" | "connecting" | "connected" | "reconnecting" | "disconnected") {
      if (stateTimerRef.current !== null) {
        window.clearTimeout(stateTimerRef.current);
        stateTimerRef.current = null;
      }

      if (nextState === "connected" || nextState === "offline") {
        setRealtimeState(nextState);
        return;
      }

      const delay = nextState === "connecting" ? 750 : 2_500;
      stateTimerRef.current = window.setTimeout(() => {
        setRealtimeState(nextState);
      }, delay);
    }

    if (!online) {
      scheduleVisibleState("offline");
      return undefined;
    }

    const hubBaseUrl = import.meta.env.VITE_SIGNALR_BASE_URL ?? getCentralBaseUrl();
    const unsubscribe = subscribeRealtime(
      {
        baseUrl: hubBaseUrl,
        token: session?.token ?? null,
      },
      (nextState) => {
        scheduleVisibleState(nextState);
      },
      (eventName) => {
        markRealtimeEvent();

        if (eventName === "BayOccupancyUpdated" || eventName === "ViolationRaised") {
          void queryClient.invalidateQueries({ queryKey: queryKeys.edgeDashboard });
        }

        if (eventName === "LotAvailabilityUpdated") {
          void queryClient.invalidateQueries({ queryKey: ["availability"] });
          void queryClient.invalidateQueries({ queryKey: queryKeys.lots.all });
        }

        if (eventName === "ReservationChanged") {
          void queryClient.invalidateQueries({ queryKey: ["availability"] });
        }
      },
    );

    return () => {
      if (stateTimerRef.current !== null) {
        window.clearTimeout(stateTimerRef.current);
        stateTimerRef.current = null;
      }
      unsubscribe();
    };
  }, [markRealtimeEvent, online, queryClient, session?.token, setRealtimeState]);
}

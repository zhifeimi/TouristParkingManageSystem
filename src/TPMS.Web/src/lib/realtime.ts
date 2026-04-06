import { HubConnection, HubConnectionBuilder, HttpTransportType, LogLevel } from "@microsoft/signalr";
import type { RealtimeState } from "../app/contexts/ConnectionStatusContext";

type RealtimeEventName = "BayOccupancyUpdated" | "LotAvailabilityUpdated" | "ReservationChanged" | "ViolationRaised";

type RealtimeConfig = {
  baseUrl: string;
  token: string | null;
};

type StateSubscriber = (state: RealtimeState) => void;
type EventSubscriber = (eventName: RealtimeEventName) => void;

const reconnectDelays = [0, 3_000, 10_000, 30_000, 60_000];
const stopGracePeriodMs = 2_000;

let connection: HubConnection | null = null;
let currentConfig: RealtimeConfig | null = null;
let startPromise: Promise<void> | null = null;
let subscriberCount = 0;
let stopTimer: number | null = null;

const stateSubscribers = new Set<StateSubscriber>();
const eventSubscribers = new Set<EventSubscriber>();

function notifyState(state: RealtimeState) {
  stateSubscribers.forEach((subscriber) => subscriber(state));
}

function notifyEvent(eventName: RealtimeEventName) {
  eventSubscribers.forEach((subscriber) => subscriber(eventName));
}

function attachEventHandlers(activeConnection: HubConnection) {
  activeConnection.on("BayOccupancyUpdated", () => notifyEvent("BayOccupancyUpdated"));
  activeConnection.on("LotAvailabilityUpdated", () => notifyEvent("LotAvailabilityUpdated"));
  activeConnection.on("ReservationChanged", () => notifyEvent("ReservationChanged"));
  activeConnection.on("ViolationRaised", () => notifyEvent("ViolationRaised"));

  activeConnection.onreconnecting(() => {
    notifyState("reconnecting");
  });

  activeConnection.onreconnected(() => {
    notifyState("connected");
  });

  activeConnection.onclose(() => {
    notifyState(typeof navigator !== "undefined" && !navigator.onLine ? "offline" : "disconnected");
  });
}

function createConnection(config: RealtimeConfig): HubConnection {
  const nextConnection = new HubConnectionBuilder()
    .withUrl(`${config.baseUrl}/hubs/occupancy`, {
      accessTokenFactory: () => currentConfig?.token ?? "",
      skipNegotiation: true,
      transport: HttpTransportType.WebSockets,
    })
    .withAutomaticReconnect(reconnectDelays)
    .configureLogging(LogLevel.Warning)
    .build();

  attachEventHandlers(nextConnection);
  return nextConnection;
}

async function stopConnection() {
  if (!connection) {
    return;
  }

  const activeConnection = connection;
  connection = null;
  startPromise = null;
  currentConfig = null;

  if (activeConnection.state !== "Disconnected") {
    await activeConnection.stop();
  }
}

async function ensureStarted(config: RealtimeConfig) {
  if (!connection) {
    currentConfig = config;
    connection = createConnection(config);
  }

  const configChanged =
    currentConfig?.baseUrl !== config.baseUrl ||
    currentConfig?.token !== config.token;

  if (configChanged) {
    await stopConnection();
    currentConfig = config;
    connection = createConnection(config);
  } else {
    currentConfig = config;
  }

  if (!connection || connection.state === "Connected" || connection.state === "Connecting" || startPromise) {
    return startPromise ?? Promise.resolve();
  }

  notifyState("connecting");
  startPromise = connection
    .start()
    .then(() => {
      notifyState("connected");
    })
    .catch(() => {
      notifyState("disconnected");
    })
    .finally(() => {
      startPromise = null;
    });

  return startPromise;
}

export function subscribeRealtime(config: RealtimeConfig, onStateChange: StateSubscriber, onEvent: EventSubscriber): () => void {
  subscriberCount += 1;
  stateSubscribers.add(onStateChange);
  eventSubscribers.add(onEvent);

  if (stopTimer !== null) {
    window.clearTimeout(stopTimer);
    stopTimer = null;
  }

  void ensureStarted(config);

  return () => {
    stateSubscribers.delete(onStateChange);
    eventSubscribers.delete(onEvent);
    subscriberCount = Math.max(0, subscriberCount - 1);

    if (subscriberCount === 0) {
      stopTimer = window.setTimeout(() => {
        if (subscriberCount === 0) {
          void stopConnection();
        }
      }, stopGracePeriodMs);
    }
  };
}

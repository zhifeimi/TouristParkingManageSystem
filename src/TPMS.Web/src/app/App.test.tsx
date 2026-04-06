import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { vi } from "vitest";
import { App } from "./App";
import { AppProviders } from "./AppProviders";

const lotResponse = [
  {
    lotId: "lot-1",
    code: "NP-01",
    name: "Summit Lake",
    timeZoneId: "Australia/Sydney",
    hourlyRate: 12,
    currency: "AUD",
  },
];

const dashboardResponse = {
  occupancy: [
    {
      bayId: "bay-1",
      bayNumber: "A-01",
      occupancyStatus: "Available",
      licensePlate: null,
      observedAtUtc: "2026-04-01T11:00:00Z",
    },
  ],
  recentLprEvents: [],
  unsyncedCount: 0,
};

function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), {
    status,
    headers: {
      "Content-Type": "application/json",
    },
  });
}

function installFetchMock() {
  vi.stubGlobal(
    "fetch",
    vi.fn((input: RequestInfo | URL) => {
      const url = typeof input === "string" ? input : input instanceof URL ? input.toString() : input.url;

      if (url.endsWith("/api/lots")) {
        return Promise.resolve(jsonResponse(lotResponse));
      }

      if (url.endsWith("/api/health") || url.endsWith("/health")) {
        return Promise.resolve(jsonResponse({ status: "ok" }));
      }

      if (url.endsWith("/api/local/controller/dashboard")) {
        return Promise.resolve(jsonResponse(dashboardResponse));
      }

      return Promise.resolve(jsonResponse({ message: `Unhandled request: ${url}` }, 500));
    }),
  );
}

function renderApp(initialEntries: string[]) {
  return render(
    <AppProviders>
      <MemoryRouter initialEntries={initialEntries}>
        <App />
      </MemoryRouter>
    </AppProviders>,
  );
}

describe("App routing", () => {
  it("renders the public home experience", async () => {
    installFetchMock();

    renderApp(["/"]);

    expect(screen.getByText(/reserve a numbered bay before you reach the park gate/i)).toBeInTheDocument();
    expect(await screen.findByText(/Summit Lake/i)).toBeInTheDocument();
  });

  it("redirects unauthenticated staff to the login screen", async () => {
    installFetchMock();

    renderApp(["/ops/controller"]);

    expect(await screen.findByText(/authenticate to continue/i)).toBeInTheDocument();
  });

  it("redirects controller-only users away from admin routes", async () => {
    installFetchMock();
    window.localStorage.setItem(
      "tpms_session",
      JSON.stringify({
        token: "token-1",
        user: {
          id: "user-1",
          email: "controller@tpms.local",
          displayName: "Controller",
          roles: ["Controller"],
        },
      }),
    );

    renderApp(["/ops/admin"]);

    expect(await screen.findByRole("heading", { name: /fast permit decisions, live occupancy, and clearer outage awareness/i })).toBeInTheDocument();
    expect(screen.queryByText(/admin overview/i)).not.toBeInTheDocument();
  });
});

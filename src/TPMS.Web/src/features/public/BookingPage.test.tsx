import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { vi } from "vitest";
import { AppProviders } from "../../app/AppProviders";
import { App } from "../../app/App";

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

const availabilityResponse = {
  lotId: "lot-1",
  lotName: "Summit Lake",
  startUtc: "2026-04-01T12:00:00Z",
  endUtc: "2026-04-01T15:00:00Z",
  totalBays: 3,
  availableBays: 1,
  occupiedBays: 1,
  reservedBays: 1,
  bays: [
    {
      bayId: "bay-1",
      bayNumber: "A-01",
      bayType: "Standard",
      isAvailable: true,
      isReserved: false,
      isOccupied: false,
      isUnderMaintenance: false,
      occupiedByLicensePlate: null,
    },
    {
      bayId: "bay-2",
      bayNumber: "A-02",
      bayType: "Accessible",
      isAvailable: false,
      isReserved: true,
      isOccupied: false,
      isUnderMaintenance: false,
      occupiedByLicensePlate: null,
    },
  ],
};

const conflictResponse = {
  error: {
    code: "conflict",
    message: "The selected bay is no longer available.",
  },
  availability: {
    ...availabilityResponse,
    availableBays: 0,
    reservedBays: 2,
    bays: availabilityResponse.bays.map((bay) =>
      bay.bayId === "bay-1"
        ? {
            ...bay,
            isAvailable: false,
            isReserved: true,
          }
        : bay,
    ),
  },
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
    vi.fn((input: RequestInfo | URL, init?: RequestInit) => {
      const url = typeof input === "string" ? input : input instanceof URL ? input.toString() : input.url;
      const method = init?.method ?? "GET";

      if (url.endsWith("/api/lots")) {
        return Promise.resolve(jsonResponse(lotResponse));
      }

      if (url.includes("/api/lots/lot-1/bays/availability")) {
        return Promise.resolve(jsonResponse(availabilityResponse));
      }

      if (url.endsWith("/api/reservations") && method === "POST") {
        return Promise.resolve(jsonResponse(conflictResponse, 409));
      }

      return Promise.resolve(jsonResponse({ message: `Unhandled request: ${method} ${url}` }, 500));
    }),
  );
}

describe("BookingPage", () => {
  it("shows refreshed availability when the selected bay is taken by another user", async () => {
    installFetchMock();

    render(
      <AppProviders>
        <MemoryRouter initialEntries={["/book"]}>
          <App />
        </MemoryRouter>
      </AppProviders>,
    );

    expect(await screen.findByText(/bay map/i)).toBeInTheDocument();
    await screen.findByRole("option", { name: /summit lake/i });
    fireEvent.click(screen.getByRole("button", { name: /refresh available bays/i }));

    fireEvent.click(await screen.findByRole("button", { name: /A-01/i }));
    fireEvent.change(screen.getByLabelText(/driver name/i), { target: { value: "Alex Visitor" } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "alex@example.com" } });
    fireEvent.change(screen.getByLabelText(/license plate/i), { target: { value: "abc 123" } });
    fireEvent.click(screen.getByRole("button", { name: /reserve and continue to payment/i }));

    await waitFor(() => {
      expect(screen.getByText(/bay selection changed/i)).toBeInTheDocument();
    });

    expect(screen.getByText(/we refreshed the bay map so you can choose a different bay/i)).toBeInTheDocument();
  });
});

import { Route, Routes } from "react-router-dom";
import { PublicShell } from "../components/layout/PublicShell";
import { OpsShell } from "../components/layout/OpsShell";
import { useRealtimeUpdates } from "../hooks/useRealtimeUpdates";
import { AdminOverviewPage } from "../features/ops/AdminOverviewPage";
import { ControllerDashboardPage } from "../features/ops/ControllerDashboardPage";
import { OpsLoginPage } from "../features/ops/OpsLoginPage";
import { BookingPage } from "../features/public/BookingPage";
import { HomePage } from "../features/public/HomePage";
import { PaymentReturnPage } from "../features/public/PaymentReturnPage";
import { ReservationPage } from "../features/public/ReservationPage";
import { ProtectedRoute } from "./routes/ProtectedRoute";
import { OpsLandingRedirect } from "./routes/OpsLandingRedirect";

function RealtimeBridge() {
  useRealtimeUpdates();
  return null;
}

export function App() {
  return (
    <>
      <RealtimeBridge />
      <Routes>
        <Route element={<PublicShell />}>
          <Route index element={<HomePage />} />
          <Route path="/book" element={<BookingPage />} />
          <Route path="/reservation/return" element={<PaymentReturnPage />} />
          <Route path="/reservation/:reservationId" element={<ReservationPage />} />
        </Route>

        <Route path="/ops/login" element={<OpsLoginPage />} />

        <Route element={<ProtectedRoute roles={["Controller", "Admin", "Operations"]} />}>
          <Route path="/ops" element={<OpsShell />}>
            <Route index element={<OpsLandingRedirect />} />
            <Route path="controller" element={<ControllerDashboardPage />} />
            <Route element={<ProtectedRoute roles={["Admin", "Operations"]} />}>
              <Route path="admin" element={<AdminOverviewPage />} />
            </Route>
          </Route>
        </Route>
      </Routes>
    </>
  );
}

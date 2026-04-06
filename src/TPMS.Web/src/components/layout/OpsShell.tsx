import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../../app/contexts/AuthContext";
import { useConnectionStatus } from "../../app/contexts/ConnectionStatusContext";
import { formatRelativeTime } from "../../lib/format";
import { Button } from "../ui/Button";
import { StatusBadge } from "../ui/StatusBadge";

export function OpsShell() {
  const navigate = useNavigate();
  const { user, hasAnyRole, signOut } = useAuth();
  const { online, realtimeState, lastRealtimeEventAt } = useConnectionStatus();

  return (
    <div className="app-frame ops-frame">
      <aside className="ops-sidebar">
        <div className="ops-brand">
          <p className="brand-kicker">Operations Workspace</p>
          <strong>TPMS Control Center</strong>
          <span>Controller and administration tools for live park operations.</span>
        </div>

        <nav className="ops-nav" aria-label="Operations navigation">
          {hasAnyRole("Controller", "Admin", "Operations") ? (
            <NavLink className={({ isActive }) => (isActive ? "ops-nav-link active" : "ops-nav-link")} to="/ops/controller">
              Controller dashboard
            </NavLink>
          ) : null}
          {hasAnyRole("Admin", "Operations") ? (
            <NavLink className={({ isActive }) => (isActive ? "ops-nav-link active" : "ops-nav-link")} to="/ops/admin">
              Admin overview
            </NavLink>
          ) : null}
        </nav>

        <div className="ops-status-stack">
          <StatusBadge tone={online ? "success" : "warning"}>{online ? "Online" : "Offline"}</StatusBadge>
          <StatusBadge tone={realtimeState === "connected" ? "info" : realtimeState === "offline" ? "warning" : "neutral"}>
            Realtime {realtimeState}
          </StatusBadge>
          <small>{lastRealtimeEventAt ? `Last event ${formatRelativeTime(lastRealtimeEventAt)}` : "Waiting for live updates"}</small>
        </div>

        <div className="ops-account">
          <strong>{user?.displayName ?? "Staff member"}</strong>
          <span>{user?.email}</span>
          <Button
            onClick={() => {
              signOut();
              navigate("/ops/login", { replace: true });
            }}
            tone="ghost"
            type="button"
          >
            Sign out
          </Button>
        </div>
      </aside>

      <main className="ops-content">
        <Outlet />
      </main>
    </div>
  );
}


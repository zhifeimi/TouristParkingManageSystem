import { NavLink, Outlet } from "react-router-dom";
import { getLastReservationId } from "../../lib/session";

const publicNavItems = [
  { to: "/", label: "Overview", end: true },
  { to: "/book", label: "Book Parking" },
];

export function PublicShell() {
  const lastReservationId = getLastReservationId();

  return (
    <div className="app-frame public-frame">
      <header className="public-header">
        <div className="brand-lockup">
          <p className="brand-kicker">National Park Access</p>
          <NavLink className="brand-link" to="/">
            Tourist Parking Management System
          </NavLink>
        </div>

        <nav className="top-nav" aria-label="Public navigation">
          {publicNavItems.map((item) => (
            <NavLink end={item.end} key={item.to} className={({ isActive }) => (isActive ? "top-nav-link active" : "top-nav-link")} to={item.to}>
              {item.label}
            </NavLink>
          ))}
          {lastReservationId ? (
            <NavLink className={({ isActive }) => (isActive ? "top-nav-link active" : "top-nav-link")} to={`/reservation/${lastReservationId}`}>
              Last reservation
            </NavLink>
          ) : null}
        </nav>

        <NavLink className="button button-primary button-md header-cta" to="/book">
          Reserve a Bay
        </NavLink>
      </header>

      <main className="page-shell">
        <Outlet />
      </main>
    </div>
  );
}

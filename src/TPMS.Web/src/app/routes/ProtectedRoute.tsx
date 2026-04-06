import { Navigate, Outlet, useLocation } from "react-router-dom";
import { defaultOpsRoute, useAuth } from "../contexts/AuthContext";

type Props = {
  roles: string[];
};

export function ProtectedRoute({ roles }: Props) {
  const location = useLocation();
  const { session, hasAnyRole } = useAuth();

  if (!session) {
    const redirect = encodeURIComponent(`${location.pathname}${location.search}`);
    return <Navigate replace to={`/ops/login?redirect=${redirect}`} />;
  }

  if (!hasAnyRole(...roles)) {
    return <Navigate replace to={defaultOpsRoute(session.user.roles)} />;
  }

  return <Outlet />;
}


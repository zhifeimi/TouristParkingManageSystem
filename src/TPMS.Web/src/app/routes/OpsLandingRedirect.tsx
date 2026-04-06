import { Navigate } from "react-router-dom";
import { defaultOpsRoute, useAuth } from "../contexts/AuthContext";

export function OpsLandingRedirect() {
  const { session } = useAuth();

  if (!session) {
    return <Navigate replace to="/ops/login" />;
  }

  return <Navigate replace to={defaultOpsRoute(session.user.roles)} />;
}


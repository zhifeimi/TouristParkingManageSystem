import { createContext, useContext, useMemo, useState, type PropsWithChildren } from "react";
import type { LoginResponse } from "../../lib/api";
import { clearStoredSession, getStoredSession, setStoredSession, type SessionUser, type StoredSession } from "../../lib/session";

type AuthContextValue = {
  session: StoredSession | null;
  user: SessionUser | null;
  isAuthenticated: boolean;
  signIn: (response: LoginResponse) => void;
  signOut: () => void;
  hasAnyRole: (...roles: string[]) => boolean;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function defaultOpsRoute(roles: string[]): string {
  if (roles.includes("Admin") || roles.includes("Operations")) {
    return "/ops/admin";
  }

  return "/ops/controller";
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<StoredSession | null>(() => getStoredSession());

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      user: session?.user ?? null,
      isAuthenticated: Boolean(session),
      signIn: (response) => {
        const nextSession = { token: response.token, user: response.user };
        setStoredSession(nextSession);
        setSession(nextSession);
      },
      signOut: () => {
        clearStoredSession();
        setSession(null);
      },
      hasAnyRole: (...roles) => {
        if (!session) {
          return false;
        }

        return roles.length === 0 || roles.some((role) => session.user.roles.includes(role));
      },
    }),
    [session],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}

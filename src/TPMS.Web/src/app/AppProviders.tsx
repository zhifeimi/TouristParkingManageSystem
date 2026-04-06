import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState, type PropsWithChildren } from "react";
import { AuthProvider } from "./contexts/AuthContext";
import { ConnectionStatusProvider } from "./contexts/ConnectionStatusContext";
import { ToastProvider } from "./contexts/ToastContext";

export function AppProviders({ children }: PropsWithChildren) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 15_000,
            refetchOnWindowFocus: false,
            retry: 1,
          },
        },
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ConnectionStatusProvider>
          <ToastProvider>{children}</ToastProvider>
        </ConnectionStatusProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}


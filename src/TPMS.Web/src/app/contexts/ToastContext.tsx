import { createContext, useContext, useMemo, useState, type PropsWithChildren } from "react";
import { ToastViewport } from "../../components/ui/ToastViewport";

export type ToastTone = "info" | "success" | "warning" | "danger";

export type Toast = {
  id: number;
  title: string;
  description?: string;
  tone: ToastTone;
};

type ToastContextValue = {
  pushToast: (toast: Omit<Toast, "id">) => void;
  dismissToast: (id: number) => void;
};

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

export function ToastProvider({ children }: PropsWithChildren) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const value = useMemo<ToastContextValue>(
    () => ({
      pushToast: (toast) => {
        const id = Date.now() + Math.round(Math.random() * 10_000);
        const nextToast = { ...toast, id };
        setToasts((current) => [...current, nextToast]);

        window.setTimeout(() => {
          setToasts((current) => current.filter((entry) => entry.id !== id));
        }, 4_500);
      },
      dismissToast: (id) => setToasts((current) => current.filter((entry) => entry.id !== id)),
    }),
    [],
  );

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastViewport toasts={toasts} onDismiss={value.dismissToast} />
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error("useToast must be used within ToastProvider.");
  }

  return context;
}


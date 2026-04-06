import type { Toast } from "../../app/contexts/ToastContext";

type Props = {
  toasts: Toast[];
  onDismiss: (id: number) => void;
};

export function ToastViewport({ toasts, onDismiss }: Props) {
  return (
    <div className="toast-viewport" aria-live="polite" aria-atomic="true">
      {toasts.map((toast) => (
        <div className={`toast toast-${toast.tone}`} key={toast.id}>
          <div>
            <strong>{toast.title}</strong>
            {toast.description ? <p>{toast.description}</p> : null}
          </div>
          <button aria-label="Dismiss notification" className="toast-dismiss" onClick={() => onDismiss(toast.id)} type="button">
            Close
          </button>
        </div>
      ))}
    </div>
  );
}


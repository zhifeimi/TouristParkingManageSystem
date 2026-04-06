import { Component, type ErrorInfo, type PropsWithChildren, type ReactNode } from "react";

type State = {
  hasError: boolean;
  errorMessage?: string;
};

export class AppErrorBoundary extends Component<PropsWithChildren, State> {
  public constructor(props: PropsWithChildren) {
    super(props);
    this.state = { hasError: false };
  }

  public static getDerivedStateFromError(error: Error): State {
    return {
      hasError: true,
      errorMessage: error.message,
    };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error("TPMS UI crashed during render.", error, errorInfo);
  }

  public render(): ReactNode {
    if (!this.state.hasError) {
      return this.props.children;
    }

    return (
      <main
        style={{
          minHeight: "100vh",
          display: "grid",
          placeItems: "center",
          padding: "2rem",
          background: "linear-gradient(180deg, #fbf7f1 0%, #f6f0e7 100%)",
          color: "#18312b",
          fontFamily: '"Aptos", "Segoe UI", sans-serif',
        }}
      >
        <section
          style={{
            width: "min(720px, 100%)",
            padding: "1.5rem",
            borderRadius: "24px",
            background: "rgba(255, 252, 247, 0.95)",
            boxShadow: "0 24px 70px rgba(32, 55, 49, 0.12)",
          }}
        >
          <p style={{ marginTop: 0, textTransform: "uppercase", letterSpacing: "0.14em", fontSize: "0.8rem", color: "#5b7168" }}>Application error</p>
          <h1 style={{ marginTop: 0 }}>The TPMS web app hit a runtime problem.</h1>
          <p style={{ lineHeight: 1.6 }}>
            Refresh the page once. If this keeps happening, the browser console message will now be much easier to inspect.
          </p>
          {this.state.errorMessage ? (
            <pre
              style={{
                padding: "1rem",
                overflowX: "auto",
                borderRadius: "16px",
                background: "#f3ede5",
                color: "#5a2f28",
                whiteSpace: "pre-wrap",
              }}
            >
              {this.state.errorMessage}
            </pre>
          ) : null}
        </section>
      </main>
    );
  }
}


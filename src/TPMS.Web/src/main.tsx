import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { App } from "./app/App";
import { AppErrorBoundary } from "./app/AppErrorBoundary";
import { AppProviders } from "./app/AppProviders";
import "./styles/index.css";

const isLocalHost =
  typeof window !== "undefined" &&
  (window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1");

try {
  if ("serviceWorker" in navigator) {
    if (import.meta.env.PROD && !isLocalHost) {
      void navigator.serviceWorker.register("/sw.js");
    } else {
      void navigator.serviceWorker.getRegistrations().then((registrations) => {
        registrations.forEach((registration) => {
          void registration.unregister();
        });
      });

      if ("caches" in globalThis) {
        void globalThis.caches.keys().then((keys) => {
          keys.forEach((key) => {
            void globalThis.caches.delete(key);
          });
        });
      }
    }
  }
} catch (error) {
  console.warn("TPMS skipped service worker setup during local bootstrap.", error);
}

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <AppErrorBoundary>
      <AppProviders>
        <BrowserRouter>
          <App />
        </BrowserRouter>
      </AppProviders>
    </AppErrorBoundary>
  </React.StrictMode>,
);

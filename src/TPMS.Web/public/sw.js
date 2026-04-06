const shellCacheName = "tpms-shell-v2";
const shellAssets = ["/", "/index.html", "/manifest.webmanifest", "/icon.svg"];
const isLocalHost = self.location.hostname === "localhost" || self.location.hostname === "127.0.0.1";

async function clearAllCaches() {
  const keys = await caches.keys();
  await Promise.all(keys.map((key) => caches.delete(key)));
}

self.addEventListener("install", (event) => {
  if (isLocalHost) {
    event.waitUntil(
      clearAllCaches().then(() => self.skipWaiting()),
    );
    return;
  }

  event.waitUntil(
    caches.open(shellCacheName).then((cache) => cache.addAll(shellAssets)).then(() => self.skipWaiting()),
  );
});

self.addEventListener("activate", (event) => {
  if (isLocalHost) {
    event.waitUntil(
      clearAllCaches()
        .then(() => self.registration.unregister())
        .then(() => self.clients.claim()),
    );
    return;
  }

  event.waitUntil(
    caches
      .keys()
      .then((keys) => Promise.all(keys.filter((key) => key !== shellCacheName).map((key) => caches.delete(key))))
      .then(() => self.clients.claim()),
  );
});

self.addEventListener("fetch", (event) => {
  if (event.request.method !== "GET") {
    return;
  }

  const requestUrl = new URL(event.request.url);
  const isSameOrigin = requestUrl.origin === self.location.origin;

  if (!isSameOrigin || isLocalHost) {
    return;
  }

  if (event.request.mode === "navigate") {
    event.respondWith(
      fetch(event.request)
        .then((response) => {
          const copy = response.clone();
          void caches.open(shellCacheName).then((cache) => cache.put("/index.html", copy));
          return response;
        })
        .catch(async () => {
          const cachedPage = await caches.match("/index.html");
          return cachedPage ?? Response.error();
        }),
    );
    return;
  }

  const isStaticAsset = ["script", "style", "image", "font"].includes(event.request.destination);

  if (isStaticAsset) {
    event.respondWith(
      caches.match(event.request).then(async (cached) => {
        if (cached) {
          return cached;
        }

        const response = await fetch(event.request);
        const copy = response.clone();
        void caches.open(shellCacheName).then((cache) => cache.put(event.request, copy));
        return response;
      }),
    );
  }
});

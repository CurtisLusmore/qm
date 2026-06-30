const CACHE = 'qm-v1';
const SCOPE = '/';

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE).then((cache) =>
      cache.addAll([SCOPE, SCOPE + 'manifest.json'])
    )
  );
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => k !== CACHE).map((k) => caches.delete(k)))
    )
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  if (!event.request.url.includes(SCOPE)) return;

  event.respondWith(
    caches.match(event.request).then((cached) => {
      const network = fetch(event.request).then((response) => {
        if (response.ok) {
          const cloned = response.clone();
          caches.open(CACHE).then((cache) => cache.put(event.request, cloned));
        }
        return response;
      });
      return cached || network;
    })
  );
});

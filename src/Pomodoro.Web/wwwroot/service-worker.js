// Pomodoro Service Worker - Production Version
//
// =============================================================================
// DEPLOYMENT VERSIONING GUIDE
// =============================================================================
// When deploying a new version, you MUST update BOTH the CACHE_NAME and
// CACHE_VERSION to ensure users receive the latest assets:
//1. Update CACHE_VERSION to the next integer (e.g., 5 -> 6)
//   2. Update CACHE_NAME to match (e.g., 'pomodoro-cache-v5' -> 'pomodoro-cache-v6')
//
// This triggers the "activate" event which cleans up old caches.
// Failing to update will cause users to see stale content until they manually
// clear their browser cache or the service worker naturally expires (24h+).
// =============================================================================
//
// Version: 7.0.0 (cache bump after refactoring)
// Last updated: 2026-04-07

const CACHE_NAME = 'pomodoro-cache-v7';
const CACHE_VERSION = 7;

// Assets to cache immediately on install (static assets)
const PRECACHE_ASSETS = [
    '/',
    '/index.html',
    '/css/app.css',
    '/css/history.css',
    '/css/settings.css',
    '/js/app-init.js',
    '/js/jsConstants.js',
    '/js/notifications.js',
    '/js/timer.js',
    '/js/chartInterop.js',
    '/js/indexedDbInterop.js',
    '/js/pipTimer.js',
    '/js/keyboard-shortcuts.js',
    '/js/infiniteScroll.js',
    '/js/fileInterop.js',
    '/icon-192.svg',
    '/icon-512.svg',
    '/manifest.webmanifest'
];

// Service Worker Constants (must be defined locally - service workers can't access main thread's window object)
const SW_CONSTANTS = {
    broadcastChannel: 'pomodoro-notifications',
    notificationActionType: 'NOTIFICATION_ACTION',
    urlParam: 'notification_action',
    defaultAction: 'default',
    logPrefix: '[ServiceWorker]'
};

// BroadcastChannel for reliable communication with page
const broadcastChannel = new BroadcastChannel(SW_CONSTANTS.broadcastChannel);

// Install event - precache static assets
self.addEventListener('install', event => {
    console.log(SW_CONSTANTS.logPrefix, 'Installing, caching static assets');
    
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                // Cache static assets individually to handle failures gracefully
                return Promise.allSettled(
                    PRECACHE_ASSETS.map(url => 
                        cache.add(url).catch(err => {
                            console.warn(SW_CONSTANTS.logPrefix, 'Failed to cache:', url, err);
                        })
                    )
                );
            })
            .then(() => {
                // Skip waiting to activate immediately
                return self.skipWaiting();
            })
    );
});

// Activate event - clean up old caches and claim clients
self.addEventListener('activate', event => {
    console.log(SW_CONSTANTS.logPrefix, 'Activating, cleaning old caches');
    
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(name => name !== CACHE_NAME)
                        .map(name => {
                            console.log(SW_CONSTANTS.logPrefix, 'Deleting old cache:', name);
                            return caches.delete(name);
                        })
                );
            })
            .then(() => {
                // Claim clients immediately
                return self.clients.claim();
            })
    );
});

// Fetch event - stale-while-revalidate strategy for better offline support
self.addEventListener('fetch', event => {
    const request = event.request;
    
    // Only handle GET requests
    if (request.method !== 'GET') {
        event.respondWith(fetch(request));
        return;
    }
    
    // Skip cross-origin requests
    if (!request.url.startsWith(self.location.origin)) {
        event.respondWith(fetch(request));
        return;
    }
    
    // Skip Blazor framework resources - always fetch fresh
    if (request.url.includes('/_framework/') || request.url.includes('/_content/')) {
        event.respondWith(fetch(request));
        return;
    }
    
    // Stale-while-revalidate strategy for static assets
    event.respondWith(
        caches.match(request)
            .then(cachedResponse => {
                // Start fetching fresh version in background
                const fetchPromise = fetch(request)
                    .then(networkResponse => {
                        // Don't cache non-successful responses
                        if (!networkResponse || networkResponse.status !== 200) {
                            return networkResponse;
                        }
                        
                        // Clone the response before caching
                        const responseToCache = networkResponse.clone();
                        caches.open(CACHE_NAME)
                            .then(cache => {
                                cache.put(request, responseToCache);
                            });
                        
                        return networkResponse;
                    })
                    .catch(err => {
                        console.warn(SW_CONSTANTS.logPrefix, 'Fetch failed:', err);
                        // Return cached response if network fails
                        return cachedResponse;
                    });
                
                // Return cached response immediately if available, otherwise wait for network
                return cachedResponse || fetchPromise;
            })
    );
});

// Handle notification click events
self.addEventListener('notificationclick', event => {
    event.notification.close();
    
    const action = event.action || SW_CONSTANTS.defaultAction;
    
    // Send message via BroadcastChannel immediately
    broadcastChannel.postMessage({
        type: SW_CONSTANTS.notificationActionType,
        action: action
    });
    
    // Focus or open the app window
    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true })
            .then(clientList => {
                // If there's already a window open, focus it and send message
                for (const client of clientList) {
                    if (client.url.includes(self.location.origin) && 'focus' in client) {
                        return client.focus().then(() => {
                            client.postMessage({
                                type: SW_CONSTANTS.notificationActionType,
                                action: action
                            });
                        });
                    }
                }
                
                // Otherwise open a new window with action parameter
                if (clients.openWindow) {
                    return clients.openWindow('/?' + SW_CONSTANTS.urlParam + '=' + encodeURIComponent(action));
                }
            })
            .catch(err => {
                console.error(SW_CONSTANTS.logPrefix + ' Error:', err);
            })
    );
});

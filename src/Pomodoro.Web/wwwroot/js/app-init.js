// Application initialization script
// This file contains service worker registration and URL helper functions
// Separated from index.html for CSP compliance

(function() {
    'use strict';
    
    // Register service worker
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('service-worker.js')
            .then(function(registration) {
                console.log('[App] Service worker registered:', registration.scope);
            })
            .catch(function(err) {
                console.error('[App] Service worker registration failed:', err);
            });
    }
    
    // Helper function to get URL parameters
    window.getUrlParameter = function(name) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    };
    
    // Helper function to remove URL parameter without page reload
    window.removeUrlParameter = function(name) {
        const url = new URL(window.location.href);
        url.searchParams.delete(name);
        window.history.replaceState({}, document.title, url.toString());
    };
})();

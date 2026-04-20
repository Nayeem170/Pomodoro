// Application initialization script
// This file contains service worker registration and URL helper functions
// Separated from index.html for CSP compliance

(function() {
    'use strict';
    
    // Service worker registration disabled - Cloudflare Pages _redirects catch-all
    // prevents serving service-worker.js with correct MIME type
    
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

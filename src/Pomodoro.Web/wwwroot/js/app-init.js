// Application initialization script
// This file contains service worker registration and URL helper functions
// Separated from index.html for CSP compliance

(function() {
    'use strict';
    
    // Service worker registration (worker ships as a static wwwroot asset;
    // _redirects serves /service-worker.js with the correct MIME type)
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', function() {
            navigator.serviceWorker.register('/service-worker.js').catch(function(err) {
                console.warn('Service worker registration failed:', err);
            });
        });
    }
    
    // Task name textareas: auto-grow on input, block Enter newline (commit is handled in Blazor)
    var taskTextareaSelector = 'textarea.task-input, textarea.task-text-input, textarea.tep-input';
    document.addEventListener('input', function(e) {
        var t = e.target;
        if (t && t.matches && t.matches(taskTextareaSelector)) {
            t.style.height = 'auto';
            var cs = getComputedStyle(t);
            var borders = parseFloat(cs.borderTopWidth) + parseFloat(cs.borderBottomWidth);
            t.style.height = (t.scrollHeight + borders) + 'px';
        }
    });
    document.addEventListener('keydown', function(e) {
        var t = e.target;
        if ((e.key === 'Enter' || e.key === 'Escape') && t && t.matches && t.matches(taskTextareaSelector)) {
            if (e.key === 'Enter') {
                e.preventDefault();
            }
            t.style.height = '';
        }
    });
    
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

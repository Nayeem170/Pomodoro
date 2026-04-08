// Infinite Scroll Interop for Blazor
// Uses Intersection Observer API to detect when elements come into view

window.infiniteScrollInterop = {
    observers: new Map(),
    loadingSentinels: new Set(),
    
    // Create an observer for a sentinel element
    // @param sentinelId - The ID of the sentinel element to observe
    // @param dotNetRef - DotNetObjectReference for callbacks
    // @param rootElementId - Optional ID of the scroll container (default: null for viewport)
    // @param rootMargin - Optional root margin for intersection detection (default: '100px')
    // @param timeoutMs - Optional timeout in milliseconds for callback (default: 10000)
    createObserver: function(sentinelId, dotNetRef, rootElementId, rootMargin, timeoutMs) {
        if (!dotNetRef) {
            console.error('InfiniteScroll: dotNetRef is required');
            return false;
        }
        
        const sentinel = document.getElementById(sentinelId);
        if (!sentinel) {
            console.warn('InfiniteScroll: Sentinel element not found:', sentinelId);
            return false;
        }
        
        // Get root element if provided (scrollable container)
        let root = null;
        if (rootElementId) {
            root = document.getElementById(rootElementId);
            if (!root) {
                console.warn('InfiniteScroll: Root element not found:', rootElementId, 'falling back to viewport');
            }
        }
        
        // Clean up existing observer if any
        this.destroyObserver(sentinelId);
        
        // Use provided rootMargin or default to '100px'
        // Can also be configured via CSS variable --scroll-sentinel-margin
        const cssVar = getComputedStyle(document.documentElement)
            .getPropertyValue('--scroll-sentinel-margin')?.trim();
        const configuredMargin = rootMargin || cssVar || '100px';
        
        // Use provided timeoutMs or default to 5000ms (5 seconds)
        // Can also be configured via CSS variable --scroll-sentinel-timeout
        const cssTimeoutVar = getComputedStyle(document.documentElement)
            .getPropertyValue('--scroll-sentinel-timeout')?.trim();
        const configuredTimeout = timeoutMs || parseInt(cssTimeoutVar) || 5000;
        
        const options = {
            root: root, // scrollable container or viewport (null)
            rootMargin: configuredMargin,
            threshold: 0.1
        };
        
        const self = this;
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && !self.isLoading(sentinelId)) {
                    // Mark as loading to prevent duplicate calls
                    self.setLoading(sentinelId, true);
                    
                    // Set a timeout to clear loading state if callback hangs
                    const timeoutId = setTimeout(() => {
                        console.warn('InfiniteScroll: Callback timeout for', sentinelId);
                        self.setLoading(sentinelId, false);
                    }, configuredTimeout);
                    
                    // Invoke Blazor callback
                    dotNetRef.invokeMethodAsync('OnSentinelIntersecting')
                        .then(() => {
                            clearTimeout(timeoutId);
                            // Clear loading state after successful callback to allow subsequent loads
                            self.setLoading(sentinelId, false);
                        })
                        .catch((err) => {
                            clearTimeout(timeoutId);
                            console.error('InfiniteScroll: Callback error for', sentinelId, err);
                            // Clear loading state on error to allow retry
                            self.setLoading(sentinelId, false);
                        });
                }
            });
        }, options);
        
        observer.observe(sentinel);
        this.observers.set(sentinelId, observer);
        
        console.log('InfiniteScroll: Observer created for', sentinelId);
        return true;
    },
    
    // Check if a sentinel is currently loading
    isLoading: function(sentinelId) {
        return this.loadingSentinels.has(sentinelId);
    },
    
    // Set loading state for a sentinel
    setLoading: function(sentinelId, loading) {
        if (loading) {
            this.loadingSentinels.add(sentinelId);
        } else {
            this.loadingSentinels.delete(sentinelId);
        }
    },
    
    // Destroy an observer
    // @param sentinelId - The ID of the sentinel element
    destroyObserver: function(sentinelId) {
        const observer = this.observers.get(sentinelId);
        if (observer) {
            observer.disconnect();
            this.observers.delete(sentinelId);
            this.loadingSentinels.delete(sentinelId);
            console.log('InfiniteScroll: Observer destroyed for', sentinelId);
        }
    },
    
    // Destroy all observers
    destroyAllObservers: function() {
        this.observers.forEach((observer, id) => {
            observer.disconnect();
        });
        this.observers.clear();
        this.loadingSentinels.clear();
        console.log('InfiniteScroll: All observers destroyed');
    },
    
    // Check if Intersection Observer API is supported
    isSupported: function() {
        return 'IntersectionObserver' in window;
    }
};

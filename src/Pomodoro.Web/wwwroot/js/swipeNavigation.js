window.swipeNavigation = {
    _handler: null,
    _dotNetRef: null,
    _startX: 0,
    _startY: 0,
    _threshold: 50,
    _transitioning: false,

    init: function(dotNetRef, routes) {
        this._dotNetRef = dotNetRef;
        this._routes = routes;
        this._handler = this._handleTouch.bind(this);
        document.addEventListener('touchstart', this._handler, { passive: true });
        document.addEventListener('touchend', this._handler, { passive: true });
    },

    dispose: function() {
        if (this._handler) {
            document.removeEventListener('touchstart', this._handler);
            document.removeEventListener('touchend', this._handler);
            this._handler = null;
        }
        this._dotNetRef = null;
    },

    _handleTouch: function(e) {
        if (!this._dotNetRef || this._transitioning) return;
        if (e.target.closest('input, textarea, select, button, a, .consent-modal, .keyboard-help-modal')) return;

        if (e.type === 'touchstart') {
            this._startX = e.changedTouches[0].clientX;
            this._startY = e.changedTouches[0].clientY;
            return;
        }

        if (e.type === 'touchend') {
            const dx = e.changedTouches[0].clientX - this._startX;
            const dy = e.changedTouches[0].clientY - this._startY;

            if (Math.abs(dx) < this._threshold || Math.abs(dy) > Math.abs(dx)) return;

            const currentPath = window.location.pathname;
            const currentIndex = this._routes.indexOf(currentPath);
            if (currentIndex === -1) return;

            if (dx < 0 && currentIndex < this._routes.length - 1) {
                this._navigate(this._routes[currentIndex + 1], 'left');
            } else if (dx > 0 && currentIndex > 0) {
                this._navigate(this._routes[currentIndex - 1], 'right');
            }
        }
    },

    _navigate: function(path, direction) {
        this._transitioning = true;

        const slideClass = direction === 'left' ? 'slide-from-right' : 'slide-from-left';

        this._dotNetRef.invokeMethodAsync('NavigateTo', path).then(() => {
            setTimeout(() => {
                const content = document.querySelector('.app-content');
                if (!content) {
                    this._transitioning = false;
                    return;
                }

                content.classList.add(slideClass);

                requestAnimationFrame(() => {
                    requestAnimationFrame(() => {
                        content.classList.remove(slideClass);
                    });
                });

                setTimeout(() => {
                    this._transitioning = false;
                }, 500);
            }, 50);
        }).catch((err) => {
            console.error('Swipe navigation failed:', err);
            this._transitioning = false;
        });
    }
};

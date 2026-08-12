// Keyboard shortcuts handler for Pomodoro WebAssembly app
window.keyboardShortcuts = (function() {
    let dotNetRef = null;
    let isInitialized = false;
    const registeredKeys = new Set();

    function isInInputField(event) {
        const target = event.target;
        return target && (
            target.tagName === 'INPUT' ||
            target.tagName === 'TEXTAREA' ||
            target.isContentEditable
        );
    }

    function handleKeyDown(event) {
        if (!isInitialized) return;

        // Don't trigger shortcuts when typing in input fields
        if (isInInputField(event)) {
            return;
        }

        var key = event.key.toLowerCase();

        // Space: play/pause (no modifier)
        if (key === ' ' || key === 'space') {
            if (registeredKeys.has('space')) {
                event.preventDefault();
                invokeShortcut('space');
            }
            return;
        }

        // Ctrl+letter shortcuts (session switches). Require Ctrl alone
        // (no Shift/Alt/Meta) so they never conflict with bare typing.
        if (event.ctrlKey && !event.shiftKey && !event.altKey && !event.metaKey) {
            var composite = 'ctrl+' + key;
            if (registeredKeys.has(composite)) {
                event.preventDefault();
                invokeShortcut(composite);
            }
            return;
        }

        // Bare keys without Ctrl/Alt/Meta (Shift allowed, e.g. "?" = Shift+/):
        // reset (r), help (?), escape.
        if (!event.ctrlKey && !event.altKey && !event.metaKey) {
            if (registeredKeys.has(key)) {
                invokeShortcut(key);
            }
        }
    }

    function invokeShortcut(key) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('HandleShortcut', key).catch(function(err) {
                console.error('Keyboard shortcut callback failed:', err);
            });
        }
    }

    return {
        initialize: function(dotNetReference) {
            if (isInitialized) {
                console.warn(pomodoroConstants.messages.keyboardShortcutsAlreadyInitialized);
                return;
            }

            dotNetRef = dotNetReference;
            document.addEventListener('keydown', handleKeyDown);
            isInitialized = true;
        },

        registerKey: function(key) {
            if (typeof key === 'string') {
                registeredKeys.add(key.toLowerCase());
            }
        },

        unregisterKey: function(key) {
            if (typeof key === 'string') {
                registeredKeys.delete(key.toLowerCase());
            }
        },

        dispose: function() {
            if (!isInitialized) return;

            document.removeEventListener('keydown', handleKeyDown);
            registeredKeys.clear();
            dotNetRef = null;
            isInitialized = false;
        }
    };
})();

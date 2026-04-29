// Keyboard shortcuts handler for Pomodoro WebAssembly app
window.keyboardShortcuts = (function() {
    let dotNetRef = null;
    let isInitialized = false;
    
    // Keys that should not trigger shortcuts when in input fields
    const INPUT_KEYS = new Set([
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
        'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'backspace', 'delete', 'enter', 'tab'
    ]);
    
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
        
        const key = event.key.toLowerCase();
        
        // Special handling for Space key (don't prevent default when in input)
        if (key === ' ' || key === 'space') {
            event.preventDefault();
            invokeShortcut('space');
            return;
        }
        
        // Handle other keys
        if (!event.ctrlKey && !event.altKey && !event.metaKey) {
            invokeShortcut(key);
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
        
        dispose: function() {
            if (!isInitialized) return;
            
            document.removeEventListener('keydown', handleKeyDown);
            dotNetRef = null;
            isInitialized = false;
        }
    };
})();

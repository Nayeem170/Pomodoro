// Notification and Sound Service for Pomodoro

// Audio context for generating sounds
let audioContext = null;

// Store .NET reference for callback
let dotNetNotificationRef = null;

// Initialize audio context on user interaction
function initAudioContext() {
    if (!audioContext) {
        try {
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
        } catch (e) {
            console.warn(pomodoroConstants.messages.audioContextFailed, e.message);
            return null;
        }
    }
    return audioContext;
}

// Invoke .NET callback for notification actions
function invokeNotificationAction(action) {
    if (dotNetNotificationRef) {
        dotNetNotificationRef.invokeMethodAsync(pomodoroConstants.jsCallbacks.onNotificationActionClick, action)
            .catch(err => console.error(pomodoroConstants.messages.dotNetCallbackFailed, err));
    }
}

// BroadcastChannel for reliable communication with service worker
let broadcastChannel = null;
try {
    broadcastChannel = new BroadcastChannel(pomodoroConstants.notifications.channel);
    broadcastChannel.onmessage = function(event) {
        if (event.data && event.data.type === pomodoroConstants.notifications.actionType) {
            invokeNotificationAction(event.data.action);
        }
    };
} catch (e) {
    console.warn(pomodoroConstants.messages.broadcastChannelNotSupported, e.message);
}

// Listen for service worker notification messages (backup method)
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.addEventListener('message', function(event) {
        if (event.data && event.data.type === pomodoroConstants.notifications.actionType) {
            invokeNotificationAction(event.data.action);
        }
    });
}

// Create namespace for notification functions
window.notificationFunctions = {
    // Register .NET reference for callbacks
    registerDotNetRef: function(dotNetRef) {
        dotNetNotificationRef = dotNetRef;
    },
    
    // Unregister .NET reference (call from DisposeAsync)
    unregisterDotNetRef: function() {
        dotNetNotificationRef = null;
    },
    
    // Request notification permission
    requestNotificationPermission: function() {
        if (!('Notification' in window)) {
            return Promise.resolve('denied');
        }
        
        if (Notification.permission === 'granted') {
            return Promise.resolve('granted');
        }
        
        return new Promise((resolve) => {
            Notification.requestPermission().then(permission => {
                resolve(permission);
            }).catch(() => {
                resolve('denied');
            });
        });
    },

    // Show browser notification with session type for appropriate actions
    // Note: Permission check is handled in C# (NotificationService.cs) to avoid duplicate checks
    showNotification: function(title, body, icon, sessionType) {
        // Use the app's SVG icon for consistent appearance across browsers
        const appIcon = pomodoroConstants.notifications.iconPath;
        
        // Determine actions based on session type
        // Note: Windows only supports max 2 actions, so we prioritize the most important ones
        // Using icons only for cleaner UI
        let actions;
        if (sessionType === 0) { // Pomodoro complete - show break options + skip
            actions = [
                { action: pomodoroConstants.actions.shortBreak, title: pomodoroConstants.notificationIcons.shortBreak },
                { action: pomodoroConstants.actions.skip, title: pomodoroConstants.notificationIcons.skip }
            ];
        } else if (sessionType === 1) { // Short Break complete - start pomodoro or skip
            actions = [
                { action: pomodoroConstants.actions.startPomodoro, title: pomodoroConstants.notificationIcons.startPomodoro },
                { action: pomodoroConstants.actions.skip, title: pomodoroConstants.notificationIcons.skip }
            ];
        } else { // Long Break complete (sessionType === 2) - start pomodoro or skip
            actions = [
                { action: pomodoroConstants.actions.startPomodoro, title: pomodoroConstants.notificationIcons.startPomodoro },
                { action: pomodoroConstants.actions.skip, title: pomodoroConstants.notificationIcons.skip }
            ];
        }
        
        // Try service worker notification first (supports action buttons)
        if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
            navigator.serviceWorker.ready.then(registration => {
                const options = {
                    body: body,
                    icon: appIcon,
                    badge: appIcon,
                    tag: pomodoroConstants.notifications.tag,
                    renotify: true,
                    requireInteraction: true,
                    vibrate: pomodoroConstants.audioSettings.vibrationPattern,
                    actions: actions,
                    data: { sessionType: sessionType }
                };
                
                registration.showNotification(title, options).catch(err => {
                    console.error(pomodoroConstants.messages.notificationsServiceWorkerFailed, err);
                    showSimpleNotification(title, body, icon, sessionType);
                });
            }).catch(err => {
                console.error(pomodoroConstants.messages.notificationsServiceWorkerReadyFailed, err);
                showSimpleNotification(title, body, icon, sessionType);
            });
        } else {
            showSimpleNotification(title, body, icon, sessionType);
        }
    },

    // Play timer complete sound (Pomodoro finished)
    playTimerCompleteSound: async function() {
        const ctx = initAudioContext();
        if (!ctx) return;
        
        // Ensure AudioContext is running (browser autoplay policy)
        if (ctx.state === 'suspended') {
            try {
                await ctx.resume();
            } catch (e) {
                console.warn(pomodoroConstants.messages.audioContextResumeFailed, e);
                return;
            }
        }
        
        // Play a pleasant chime pattern using constants
        const freq = pomodoroConstants.audioFrequencies.pomodoroChime;
        playBeep(freq[0], 0.15, 'sine'); // C5
        setTimeout(() => playBeep(freq[1], 0.15, 'sine'), 150); // E5
        setTimeout(() => playBeep(freq[2], 0.15, 'sine'), 300); // G5
        setTimeout(() => playBeep(freq[3], 0.3, 'sine'), 450); // C6
        
        // Repeat pattern
        setTimeout(() => {
            playBeep(freq[0], 0.15, 'sine');
            setTimeout(() => playBeep(freq[1], 0.15, 'sine'), 150);
            setTimeout(() => playBeep(freq[2], 0.15, 'sine'), 300);
            setTimeout(() => playBeep(freq[3], 0.3, 'sine'), 450);
        }, 900);
    },

    // Play break complete sound
    playBreakCompleteSound: async function() {
        const ctx = initAudioContext();
        if (!ctx) return;
        
        // Ensure AudioContext is running (browser autoplay policy)
        if (ctx.state === 'suspended') {
            try {
                await ctx.resume();
            } catch (e) {
                console.warn(pomodoroConstants.messages.audioContextResumeFailed, e);
                return;
            }
        }
        
        // Play a gentle attention sound using constants
        const freq = pomodoroConstants.audioFrequencies.breakChime;
        playBeep(freq[0], 0.2, 'sine'); // A4
        setTimeout(() => playBeep(freq[1], 0.2, 'sine'), 200); // C#5
        setTimeout(() => playBeep(freq[2], 0.3, 'sine'), 400); // E5
        
        // Repeat
        setTimeout(() => {
            playBeep(freq[0], 0.2, 'sine');
            setTimeout(() => playBeep(freq[1], 0.2, 'sine'), 200);
            setTimeout(() => playBeep(freq[2], 0.3, 'sine'), 400);
        }, 800);
    },
    
    // Unlock audio context on user interaction (call this when timer starts)
    unlockAudio: function() {
        const ctx = initAudioContext();
        if (ctx && ctx.state === 'suspended') {
            ctx.resume().then(() => {
                console.log(pomodoroConstants.messages.audioContextUnlocked);
            }).catch(err => {
                console.warn(pomodoroConstants.messages.audioContextUnlockFailed, err);
            });
        }
    }
};

// Simple notification fallback (without actions) for non-service-worker contexts
function showSimpleNotification(title, body, icon, sessionType) {
    // Use the app's SVG icon for consistent appearance across browsers
    const appIcon = pomodoroConstants.notifications.iconPath;
    
    const options = {
        body: body,
        icon: appIcon, // Large notification icon
        badge: appIcon, // Small icon beside site name
        tag: pomodoroConstants.notifications.tag,
        renotify: true,
        requireInteraction: true
    };
    
    const notification = new Notification(title, options);
    
    notification.onclick = function() {
        window.focus();
        // For simple notification click, use default action based on session type
        let defaultAction = sessionType === 0 ? pomodoroConstants.actions.shortBreak : pomodoroConstants.actions.startPomodoro;
        invokeNotificationAction(defaultAction);
        notification.close();
    };
    
    // Auto close after timeout if not interacted
    setTimeout(() => {
        notification.close();
    }, pomodoroConstants.notifications.timeout);
}

// Generate a beep sound using Web Audio API (internal function)
function playBeep(frequency, duration, type) {
    try {
        const ctx = initAudioContext();
        
        if (!ctx) {
            return; // AudioContext not available
        }
        
        if (ctx.state === 'suspended') {
            ctx.resume().catch(() => {});
        }
        
        const oscillator = ctx.createOscillator();
        const gainNode = ctx.createGain();
        
        oscillator.connect(gainNode);
        gainNode.connect(ctx.destination);
        
        oscillator.frequency.value = frequency;
        oscillator.type = type || 'sine';
        
        // Envelope for smoother sound
        gainNode.gain.setValueAtTime(0, ctx.currentTime);
        gainNode.gain.linearRampToValueAtTime(pomodoroConstants.audioSettings.defaultGain, ctx.currentTime + 0.01);
        gainNode.gain.linearRampToValueAtTime(pomodoroConstants.audioSettings.defaultGain, ctx.currentTime + duration - 0.01);
        gainNode.gain.linearRampToValueAtTime(0, ctx.currentTime + duration);
        
        oscillator.start(ctx.currentTime);
        oscillator.stop(ctx.currentTime + duration);
    } catch (e) {
        // Silently fail if audio is not available
        console.warn(pomodoroConstants.messages.audioPlaybackFailed, e.message);
    }
}

/**
 * JavaScript Constants for Pomodoro Application
 * These values are initialized from C# and should be kept in sync with Constants.cs
 * Call window.pomodoroConstants.initialize() from C# to update with user settings
 */
window.pomodoroConstants = {
    // Time conversions
    secondsPerMinute: 60,
    minutesPerHour: 60,
    timerIntervalMs: 1000,
    
    // Default timer durations (can be overridden by user settings)
    defaultPomodoroMinutes: 25,
    defaultShortBreakMinutes: 5,
    defaultLongBreakMinutes: 15,
    
    // UI dimensions
    pipWindowWidth: 320,
    pipWindowHeight: 240,
    
    // Chart colors
    chartColors: {
        defaultBar: 'rgba(217, 85, 85, 1)',
        highlightBar: 'rgba(59, 130, 246, 1)',
        defaultBorder: 'rgba(217, 85, 85, 1)',
        highlightBorder: 'rgba(59, 130, 246, 1)',
        breakBar: 'rgba(16, 185, 129, 1)',
        breakBorder: 'rgba(16, 185, 129, 1)',
        highlightBreakBar: 'rgba(245, 158, 11, 1)',
        highlightBreakBorder: 'rgba(245, 158, 11, 1)',
        tooltipBackground: 'rgba(40, 40, 40, 0.95)',
        gridColor: 'rgba(107, 114, 128, 0.3)',
        tickColor: '#9ca3af',
        white: '#ffffff',
        legendLabel: '#9ca3af'
    },
    
    // Chart styling
    chartStyling: {
        borderRadius: 4,
        barThickness: 28,
        barThicknessMedium: 16,
        barThicknessSmall: 12,
        tooltipPadding: 10,
        tooltipCornerRadius: 8,
        doughnutCutout: '60%',
        legendPadding: 15,
        doughnutTooltipPadding: 12,
        centerTextFont: 'bold 16px system-ui, -apple-system, sans-serif',
        doughnutBorderWidth: 2,
        doughnutHoverOffset: 4
    },
    
    // Doughnut chart color palette
    doughnutColors: {
        // Background colors (solid for better visibility)
        red: 'rgba(217, 85, 85, 1)',
        blue: 'rgba(59, 130, 246, 1)',
        green: 'rgba(16, 185, 129, 1)',
        purple: 'rgba(139, 92, 246, 1)',
        amber: 'rgba(245, 158, 11, 1)',
        pink: 'rgba(236, 72, 153, 1)',
        teal: 'rgba(20, 184, 166, 1)',
        indigo: 'rgba(99, 102, 241, 1)',
        // Border colors (solid)
        redBorder: 'rgba(217, 85, 85, 1)',
        blueBorder: 'rgba(59, 130, 246, 1)',
        greenBorder: 'rgba(16, 185, 129, 1)',
        purpleBorder: 'rgba(139, 92, 246, 1)',
        amberBorder: 'rgba(245, 158, 11, 1)',
        pinkBorder: 'rgba(236, 72, 153, 1)',
        tealBorder: 'rgba(20, 184, 166, 1)',
        indigoBorder: 'rgba(99, 102, 241, 1)',
        // Convenience arrays
        backgrounds: [
            'rgba(217, 85, 85, 1)',
            'rgba(59, 130, 246, 1)',
            'rgba(16, 185, 129, 1)',
            'rgba(139, 92, 246, 1)',
            'rgba(245, 158, 11, 1)',
            'rgba(236, 72, 153, 1)',
            'rgba(20, 184, 166, 1)',
            'rgba(99, 102, 241, 1)'
        ],
        borders: [
            'rgba(217, 85, 85, 1)',
            'rgba(59, 130, 246, 1)',
            'rgba(16, 185, 129, 1)',
            'rgba(139, 92, 246, 1)',
            'rgba(245, 158, 11, 1)',
            'rgba(236, 72, 153, 1)',
            'rgba(20, 184, 166, 1)',
            'rgba(99, 102, 241, 1)'
        ]
    },
    
    // Notification settings
    notifications: {
        timeout: 60000,
        tag: 'pomodoro-timer',
        channel: 'pomodoro-notifications',
        iconPath: '/icon-192.svg',
        actionType: 'NOTIFICATION_ACTION',
        permissionDenied: 'denied',
        permissionGranted: 'granted',
        urlParam: 'notification_action'
    },
    
    // Audio frequencies (in Hz)
    audioFrequencies: {
        // Pomodoro complete chime (C major chord: C5, E5, G5, C6)
        pomodoroChime: [523.25, 659.25, 783.99, 1046.50],
        // Break complete sound (A major chord: A4, C#5, E5)
        breakChime: [440, 554.37, 659.25]
    },
    
    // Audio settings
    audioSettings: {
        defaultGain: 0.3,
        vibrationPattern: [200, 100, 200, 100, 200],
        stateSuspended: 'suspended'
    },
    
    // Storage names (must match Constants.cs)
    storage: {
        dbName: 'PomodoroDB',
        dbVersion: 1,
        tasksStore: 'tasks',
        activitiesStore: 'activities',
        dailyStatsStore: 'dailyStats',
        settingsStore: 'settings',
        appStateStore: 'appState',
        indexes: {
            id: 'id',
            name: 'name',
            createdAt: 'createdAt',
            completedAt: 'completedAt',
            type: 'type',
            taskId: 'taskId',
            date: 'date',
            isCompleted: 'isCompleted',
            lastWorkedOn: 'lastWorkedOn',
            isDeleted: 'isDeleted'
        }
    },
    
    // Action names (must match Constants.cs)
    actions: {
        shortBreak: 'short-break',
        longBreak: 'long-break',
        startPomodoro: 'start-pomodoro',
        skip: 'skip',
        default: 'default'
    },
    
    // Session CSS class names (must match Constants.cs SessionTypes)
    // Used by both main window (Blazor) and PIP window for consistent styling
    sessionClasses: {
        pomodoro: 'pomodoro',
        shortBreak: 'short-break',
        longBreak: 'long-break',
        // Array form for index-based lookup (SessionType enum: 0=Pomodoro, 1=ShortBreak, 2=LongBreak)
        byIndex: ['pomodoro', 'short-break', 'long-break']
    },
    
    // Theme CSS class names for container backgrounds (must match Constants.cs *Theme constants)
    themeClasses: {
        pomodoro: 'pomodoro-theme',
        shortBreak: 'short-break-theme',
        longBreak: 'long-break-theme',
        // Array form for index-based lookup
        byIndex: ['pomodoro-theme', 'short-break-theme', 'long-break-theme']
    },
    
    // Get session CSS class by session type index
    getSessionClass: function(sessionTypeIndex) {
        return this.sessionClasses.byIndex[sessionTypeIndex] || this.sessionClasses.pomodoro;
    },
    
    // Get theme CSS class by session type index (for container backgrounds)
    getThemeClass: function(sessionTypeIndex) {
        return this.themeClasses.byIndex[sessionTypeIndex] || this.themeClasses.pomodoro;
    },
    
    // PiP (Picture-in-Picture) settings
    pip: {
        apiName: 'documentPictureInPicture',
        broadcastChannel: 'pomodoro-pip',
        popupName: 'PomodoroTimer',
        messages: {
            toggleTimer: 'TOGGLE_TIMER',
            resetTimer: 'RESET_TIMER',
            switchSession: 'SWITCH_SESSION',
            timerUpdate: 'TIMER_UPDATE'
        },
        callbacks: {
            onToggle: 'OnPipToggleTimer',
            onReset: 'OnPipResetTimer',
            onSwitchSession: 'OnPipSwitchSession',
            onClosed: 'OnPipClosed'
        }
    },
    
    // JS Invokable method names (must match Constants.cs JsInvokableMethods)
    jsCallbacks: {
        onTimerTick: 'OnTimerTickJs',
        onNotificationActionClick: 'OnNotificationActionClick'
    },
    
    // Chart types and labels
    chartTypes: {
        bar: 'bar',
        doughnut: 'doughnut'
    },
    
    activityLabels: {
        pomodoros: 'Pomodoros',
        shortBreaks: 'Short Breaks',
        longBreaks: 'Long Breaks',
        focusTime: 'Focus time',
        focusTimeLabel: 'Focus Time',
        breakTimeLabel: 'Break Time',
        pomodoro: 'pomodoro',
        pomodoroPlural: 'pomodoros',
        shortBreak: 's'
    },
    
    // Log messages
    messages: {
        audioContextFailed: 'Failed to create AudioContext:',
        audioContextResumeFailed: 'Failed to resume AudioContext:',
        dotNetCallbackFailed: '[NotificationsJS] .NET callback failed:',
        broadcastChannelNotSupported: '[NotificationsJS] BroadcastChannel not supported:',
        chartNotLoaded: 'Chart.js is not loaded. Make sure Chart.js CDN is included in index.html',
        chartNotLoadedShort: 'Chart.js is not loaded!',
        canvasNotFound: 'Canvas not found:',
        pipWindowOpened: '[PiP] Document PiP window opened',
        pipFailed: '[PiP] Document PiP failed:',
        pipPopupBlocked: '[PiP] Popup blocked',
        pipPopupOpened: '[PiP] Popup window opened',
        timerTickError: 'Timer tick error: ',
        serviceWorkerError: '[ServiceWorker] Error:',
        chartInteropPrefix: '[ChartInterop]',
        chartInteropCreateDoughnut: '[ChartInterop] createDoughnutChart called:',
        chartInteropCanvasNotFound: '[ChartInterop] Canvas not found:',
        chartInteropCanvasFound: '[ChartInterop] Canvas found, dimensions:',
        chartInteropNotLoaded: '[ChartInterop] Chart.js is not loaded!',
        indexedDbPrefix: '[IndexedDB]',
        indexedDbOpenFailed: '[IndexedDB] Failed to open database:',
        indexedDbOpened: '[IndexedDB] Database opened successfully',
        indexedDbUpgrading: '[IndexedDB] Upgrading database schema...',
        indexedDbCreatedTasksStore: '[IndexedDB] Created tasks store',
        keyboardShortcutsAlreadyInitialized: 'Keyboard shortcuts already initialized',
        keyboardShortcutsInitialized: 'Keyboard shortcuts initialized',
        keyboardShortcutsDisposed: 'Keyboard shortcuts disposed',
        audioContextUnlocked: 'AudioContext unlocked successfully',
        audioContextUnlockFailed: 'Failed to unlock AudioContext:',
        indexedDbCreatedActivitiesStore: '[IndexedDB] Created activities store',
        indexedDbCreatedDailyStatsStore: '[IndexedDB] Created dailyStats store',
        indexedDbCreatedSettingsStore: '[IndexedDB] Created settings store',
        indexedDbCreatedAppStateStore: '[IndexedDB] Created appState store',
        indexedDbErrorPuttingItem: '[IndexedDB] Error putting item in',
        indexedDbExceptionPutItem: '[IndexedDB] Exception in putItem:',
        indexedDbErrorGettingItem: '[IndexedDB] Error getting item from',
        indexedDbExceptionGetItem: '[IndexedDB] Exception in getItem:',
        indexedDbErrorGettingAllItems: '[IndexedDB] Error getting all items from',
        indexedDbExceptionGetAllItems: '[IndexedDB] Exception in getAllItems:',
        indexedDbErrorQueryingIndex: '[IndexedDB] Error querying index',
        indexedDbExceptionGetItemsByIndex: '[IndexedDB] Exception in getItemsByIndex:',
        indexedDbErrorQueryingDateRange: '[IndexedDB] Error querying date range:',
        indexedDbExceptionGetItemsByDateRange: '[IndexedDB] Exception in getItemsByDateRange:',
        indexedDbErrorDeletingItem: '[IndexedDB] Error deleting item from',
        indexedDbExceptionDeleteItem: '[IndexedDB] Exception in deleteItem:',
        indexedDbBatchPutCompletedWithErrors: '[IndexedDB] Batch put completed with',
        indexedDbTransactionError: '[IndexedDB] Transaction error in putAllItems:',
        indexedDbExceptionPutAllItems: '[IndexedDB] Exception in putAllItems:',
        indexedDbErrorClearingStore: '[IndexedDB] Error clearing store',
        indexedDbExceptionClearStore: '[IndexedDB] Exception in clearStore:',
        notificationsServiceWorkerFailed: '[NotificationsJS] Service worker notification failed:',
        notificationsServiceWorkerReadyFailed: '[NotificationsJS] Service worker ready failed:',
        audioPlaybackFailed: 'Audio playback failed:'
    },
    
    // Notification action icons (emojis)
    notificationIcons: {
        shortBreak: '☕',
        skip: '⏭️',
        startPomodoro: '🍅'
    },
    
    // Chart position settings
    chartPositions: {
        legendBottom: 'bottom',
        legendTop: 'top',
        legendRight: 'right',
        legendAlignStart: 'start',
        cutoutCircle: 'circle',
        pointStyleRectRounded: 'rectRounded',
        textAlignCenter: 'center',
        textAlignLeft: 'left',
        textBaselineMiddle: 'middle'
    },
    
    // Chart plugin IDs
    chartPlugins: {
        centerText: 'centerText'
    },
    
    /**
     * Initialize constants with user settings from C#
     * @param {object} settings - Timer settings from Blazor
     */
    initialize: function(settings) {
        if (settings) {
            if (settings.pomodoroMinutes) {
                this.defaultPomodoroMinutes = settings.pomodoroMinutes;
            }
            if (settings.shortBreakMinutes) {
                this.defaultShortBreakMinutes = settings.shortBreakMinutes;
            }
            if (settings.longBreakMinutes) {
                this.defaultLongBreakMinutes = settings.longBreakMinutes;
            }
        }
        console.log('Pomodoro constants initialized:', this.defaultPomodoroMinutes, 'min pomodoro');
    },
    
    /**
     * Format minutes into hours and minutes string
     * @param {number} totalMinutes - Total minutes to format
     * @returns {string} Formatted string like "1h 30m" or "45m"
     */
    formatTime: function(totalMinutes) {
        if (totalMinutes < this.minutesPerHour) {
            return totalMinutes + 'm';
        }
        const hours = Math.floor(totalMinutes / this.minutesPerHour);
        const mins = totalMinutes % this.minutesPerHour;
        return mins > 0 ? hours + 'h ' + mins + 'm' : hours + 'h';
    },
    
    /**
     * Calculate focus time from pomodoro count using current settings
     * @param {number} pomodoroCount - Number of completed pomodoros
     * @returns {number} Total focus minutes
     */
    calculateFocusTime: function(pomodoroCount) {
        return pomodoroCount * this.defaultPomodoroMinutes;
    }
};

// Picture-in-Picture Timer for Pomodoro
// Provides a floating, always-on-top timer window

window.pipTimer = {
    pipWindow: null,
    pipDocument: null,
    broadcastChannel: null,
    dotNetRef: null,
    pipScriptInitialized: false,
    
    // Check if Document PiP API is supported
    isSupported: function() {
        return pomodoroConstants.pip.apiName in window;
    },
    
    // Check if PiP window is currently open
    isOpen: function() {
        return this.pipWindow && !this.pipWindow.closed;
    },
    
    // Register .NET reference for callbacks
    registerDotNetRef: function(dotNetRef) {
        this.dotNetRef = dotNetRef;
    },
    
    // Unregister .NET reference
    unregisterDotNetRef: function() {
        this.dotNetRef = null;
    },
    
    // Open PiP window
    open: async function(timerState) {
        // Close existing window if open
        if (this.isOpen()) {
            this.close();
            return false;
        }
        
        // Try Document PiP API first
        if (this.isSupported()) {
            try {
                this.pipWindow = await window[pomodoroConstants.pip.apiName].requestWindow({
                    width: pomodoroConstants.pipWindowWidth,
                    height: pomodoroConstants.pipWindowHeight
                });
                
                this.pipDocument = this.pipWindow.document;
                this.copyStyles();
                this.setupCloseHandler();
                this.renderTimer(timerState);
                this.setupBroadcastChannel();
                
                console.log(pomodoroConstants.messages.pipWindowOpened);
                return true;
            } catch (e) {
                console.warn(pomodoroConstants.messages.pipFailed, e.message);
            }
        }
        
        // Fallback to popup window
        return this.openFallback(timerState);
    },
    
    // Fallback: Open as popup window
    openFallback: function(timerState) {
        const features = [
            'width=' + pomodoroConstants.pipWindowWidth,
            'height=' + pomodoroConstants.pipWindowHeight,
            'menubar=no',
            'toolbar=no',
            'location=no',
            'status=no',
            'resizable=yes',
            'scrollbars=no'
        ].join(',');
        
        this.pipWindow = window.open('', pomodoroConstants.pip.popupName, features);
        
        if (!this.pipWindow) {
            console.error(pomodoroConstants.messages.pipPopupBlocked);
            return false;
        }
        
        this.pipDocument = this.pipWindow.document;
        this.writePopupContent(timerState);
        this.setupCloseHandler();
        this.setupBroadcastChannel();
        
        console.log(pomodoroConstants.messages.pipPopupOpened);
        return true;
    },
    
    // Copy styles from main document to PiP window
    copyStyles: function() {
        if (!this.pipDocument) return;
        
        // Copy all style sheets
        const styleSheets = document.querySelectorAll('link[rel="stylesheet"], style');
        styleSheets.forEach(sheet => {
            try {
                if (sheet.tagName === 'STYLE') {
                    const clone = sheet.cloneNode(true);
                    this.pipDocument.head.appendChild(clone);
                } else if (sheet.tagName === 'LINK') {
                    const clone = sheet.cloneNode(true);
                    this.pipDocument.head.appendChild(clone);
                }
            } catch (e) {
                // Ignore cross-origin stylesheet errors
            }
        });
        
        // Add PiP-specific styles (compact version)
        const pipStyles = this.pipDocument.createElement('style');
        pipStyles.textContent = `
            :root {
                --surface-color: #16213e;
            }
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body {
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                color: #ffffff;
                min-height: 100vh;
                display: flex;
                flex-direction: column;
                align-items: center;
                justify-content: center;
                overflow: hidden;
                padding: 0;
            }
            .pip-container {
                text-align: center;
                width: 100%;
                min-height: 100vh;
                display: flex;
                flex-direction: column;
                align-items: center;
                justify-content: center;
            }
            .pip-container.pomodoro-theme {
                background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, var(--surface-color) 100%);
            }
            .pip-container.short-break-theme {
                background: linear-gradient(135deg, rgba(39, 174, 96, 0.15) 0%, var(--surface-color) 100%);
            }
            .pip-container.long-break-theme {
                background: linear-gradient(135deg, rgba(52, 152, 219, 0.15) 0%, var(--surface-color) 100%);
            }
            .session-tabs {
                display: flex;
                justify-content: center;
                gap: 1px;
                margin-bottom: 2px;
            }
            .session-tab {
                padding: 1px 2px;
                border: none;
                border-radius: 3px;
                font-size: 5px;
                font-weight: 600;
                cursor: pointer;
                text-transform: uppercase;
                letter-spacing: 0;
                transition: all 0.2s;
                background: rgba(255,255,255,0.1);
                color: #a0a0a0;
            }
            .session-tab.active {
                background: var(--session-color, #e74c3c);
                color: white;
            }
            .session-tab.pomodoro { --session-color: #e74c3c; }
            .session-tab.short-break { --session-color: #27ae60; }
            .session-tab.long-break { --session-color: #3498db; }
            .time-display {
                font-size: 70px;
                font-weight: 700;
                font-family: 'Courier New', monospace;
                font-variant-numeric: tabular-nums;
                letter-spacing: 4px;
                margin: 2px 0;
            }
            .time-display { color: #ffffff; }
            .time-display.paused { opacity: 0.7; }
            .time-display.running.pomodoro { color: #e74c3c; }
            .time-display.running.short-break { color: #27ae60; }
            .time-display.running.long-break { color: #3498db; }
            .controls {
                display: flex;
                justify-content: center;
                gap: 10px;
                margin-top: -10px;
            }
            .btn {
                width: 50px;
                height: 50px;
                border: none;
                border-radius: 50%;
                font-size: 1.2rem;
                cursor: pointer;
                transition: all 0.2s;
                display: inline-flex;
                align-items: center;
                justify-content: center;
                line-height: 1;
                padding: 0;
            }
            .btn-primary {
                background: var(--session-color, #e74c3c);
                color: white;
                box-shadow: 0 4px 15px rgba(231, 76, 60, 0.4),
                            0 2px 6px rgba(231, 76, 60, 0.3);
            }
            .btn-primary:hover {
                filter: brightness(1.1);
                transform: scale(1.05);
            }
            .btn-primary.pomodoro { background: #e74c3c; }
            .btn-primary.short-break { background: #27ae60; }
            .btn-primary.long-break { background: #3498db; }
            .btn-pause {
                background: #f39c12;
                color: white;
                box-shadow: 0 4px 15px rgba(243, 156, 18, 0.4),
                            0 2px 6px rgba(243, 156, 18, 0.3);
            }
            .btn-pause:hover {
                background: #e67e22;
                transform: scale(1.05);
            }
            .btn-secondary {
                background-color: #7f8c8d;
                color: white;
                box-shadow: 0 4px 15px rgba(127, 140, 141, 0.4),
                            0 2px 6px rgba(127, 140, 141, 0.3);
            }
            .btn-secondary:hover {
                transform: translateY(-2px);
                background-color: #6c7a7b;
            }
            .current-task {
                margin-top: 4px;
                padding: 3px 6px;
                font-size: 8px;
                color: rgba(255,255,255,0.9);
                background: rgba(217, 85, 85, 0.3);
                border-radius: 6px;
                border-left: 2px solid #d95555;
                max-width: 180px;
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: nowrap;
                text-align: left;
            }
        `;
        this.pipDocument.head.appendChild(pipStyles);
    },
    
    // Write content for popup fallback
    writePopupContent: function(timerState) {
        if (!this.pipDocument) return;
        
        this.pipDocument.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <title>Pomodoro Timer</title>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
            </head>
            <body>
                <div class="pip-container" id="pip-container">
                    ${this.generateTimerHTML(timerState)}
                </div>
            </body>
            </html>
        `);
        this.pipDocument.close();
        
        // Add styles after document is written
        this.copyStyles();
    },
    
    // Generate timer HTML
    generateTimerHTML: function(state) {
        const sessionType = state.sessionType || 0;
        // Use shared sessionClasses from pomodoroConstants (DRY principle)
        const sessionClass = pomodoroConstants.getSessionClass(sessionType);
        
        const remainingSeconds = state.remainingSeconds ?? 0;
        const minutes = Math.floor(remainingSeconds / 60);
        const seconds = remainingSeconds % 60;
        const timeDisplay = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        
        const isRunning = state.isRunning || false;
        const isStarted = state.isStarted || false;
        const playPauseIcon = isRunning ? '⏸' : '▶';
        
        // Single source of truth: showReset is computed by C# GetTimerState()
        // Business logic: showReset = isRunning || isStarted (kept in C# only)
        const showReset = state.showReset || false;
        
        return `
            <div class="session-tabs">
                <button class="session-tab pomodoro ${sessionType === 0 ? 'active' : ''}"
                        onclick="window.pipSwitchSession(0)">Pomo</button>
                <button class="session-tab short-break ${sessionType === 1 ? 'active' : ''}"
                        onclick="window.pipSwitchSession(1)">Short</button>
                <button class="session-tab long-break ${sessionType === 2 ? 'active' : ''}"
                        onclick="window.pipSwitchSession(2)">Long</button>
            </div>
            <div class="time-display ${isRunning ? 'running' : 'paused'} ${sessionClass}">${timeDisplay}</div>
            <div class="controls">
                <button class="btn ${isRunning ? 'btn-pause' : `btn-primary ${sessionClass}`}"
                        onclick="window.pipToggleTimer()">${playPauseIcon}</button>
                ${showReset ? `<button class="btn btn-secondary"
                        onclick="window.pipResetTimer()">↺</button>` : ''}
            </div>
        `;
    },
    
    // Render timer in PiP window
    renderTimer: function(timerState) {
        if (!this.pipDocument) return;
        
        const sessionType = timerState.sessionType || 0;
        // Use shared themeClasses from pomodoroConstants (DRY principle)
        const themeClass = pomodoroConstants.getThemeClass(sessionType);
        
        // Get or create container
        let container = this.pipDocument.getElementById('pip-container');
        if (!container) {
            container = this.pipDocument.createElement('div');
            container.id = 'pip-container';
            this.pipDocument.body.appendChild(container);
        }
        
        // Ensure global functions are available (pipToggleTimer, pipResetTimer, pipSwitchSession)
        // These functions are needed for button onclick handlers
        // Only initialize once to avoid repeated checks
        if (!this.pipScriptInitialized) {
            this.ensurePipScript();
            this.pipScriptInitialized = true;
        }
        
        // Update container class for theme (gradient applied to container, not body)
        container.className = 'pip-container ' + themeClass;
        container.innerHTML = this.generateTimerHTML(timerState);
        
        // Ensure body has tabindex and focus for keyboard shortcuts
        // This is needed because innerHTML replacement doesn't preserve focus state
        if (this.pipDocument.body) {
            this.pipDocument.body.setAttribute('tabindex', '0');
            this.pipDocument.body.focus();
        }
    },
    
    // Ensure PIP script with global functions is present
    ensurePipScript: function() {
        if (!this.pipDocument) return;
        
        // Check if script already exists
        const existingScript = this.pipDocument.getElementById('pip-control-script');
        if (existingScript) {
            return; // Script already exists
        }
        
        // Add script for communication with main window
        // Note: PiP window runs in isolated context, so we inject constant values directly
        const bcName = pipTimer.getBroadcastChannelName();
        const msgToggleTimer = pipTimer.getMessageType('toggleTimer');
        const msgResetTimer = pipTimer.getMessageType('resetTimer');
        const msgSwitchSession = pipTimer.getMessageType('switchSession');
        const msgTimerUpdate = pipTimer.getMessageType('timerUpdate');
        
        const pipScript = this.pipDocument.createElement('script');
        pipScript.id = 'pip-control-script';
        pipScript.textContent = `
            // Setup broadcast channel for communication with main window
            (function() {
                var bc = new BroadcastChannel('${bcName}');
                
                // Control functions that send messages to main window
                window.pipToggleTimer = function() {
                    bc.postMessage({ type: '${msgToggleTimer}' });
                };
                window.pipResetTimer = function() {
                    bc.postMessage({ type: '${msgResetTimer}' });
                };
                window.pipSwitchSession = function(sessionType) {
                    bc.postMessage({ type: '${msgSwitchSession}', sessionType: sessionType });
                };
                
                // Listen for timer updates from main window
                bc.onmessage = function(event) {
                    if (event.data.type === '${msgTimerUpdate}') {
                        // Re-render will be handled by parent's update call
                    }
                };
                
                // Keyboard shortcuts for PIP window
                // Use window instead of document for better event handling
                window.addEventListener('keydown', function(event) {
                    // Ignore if user is typing in an input field
                    if (event.target.tagName === 'INPUT' || event.target.tagName === 'TEXTAREA') {
                        return;
                    }
                    
                    var key = event.key.toLowerCase();
                    
                    // Space: Toggle timer (play/pause)
                    if (key === ' ' || event.code === 'Space') {
                        event.preventDefault();
                        event.stopPropagation();
                        window.pipToggleTimer();
                    }
                    // R: Reset timer
                    else if (key === 'r') {
                        event.preventDefault();
                        event.stopPropagation();
                        window.pipResetTimer();
                    }
                    // P: Switch to Pomodoro
                    else if (key === 'p') {
                        event.preventDefault();
                        event.stopPropagation();
                        window.pipSwitchSession(0);
                    }
                    // S: Switch to Short Break
                    else if (key === 's') {
                        event.preventDefault();
                        event.stopPropagation();
                        window.pipSwitchSession(1);
                    }
                    // L: Switch to Long Break
                    else if (key === 'l') {
                        event.preventDefault();
                        event.stopPropagation();
                        window.pipSwitchSession(2);
                    }
                }, { capture: true });
                
                // Auto-focus PIP window to enable keyboard shortcuts immediately
                document.body.setAttribute('tabindex', '0');
                document.body.focus();
            })();
        `;
        this.pipDocument.head.appendChild(pipScript);
    },
    
    // Update timer display
    update: function(timerState) {
        if (!this.isOpen()) return;
        this.renderTimer(timerState);
    },
    
    // Setup close handler
    setupCloseHandler: function() {
        if (!this.pipWindow) return;
        
        this.pipWindow.addEventListener('pagehide', () => {
            this.pipWindow = null;
            this.pipDocument = null;
            this.notifyClosed();
        });
    },
    
    // Setup broadcast channel for communication
    setupBroadcastChannel: function() {
        if ('BroadcastChannel' in window) {
            this.broadcastChannel = new BroadcastChannel(pomodoroConstants.pip.broadcastChannel);
            this.broadcastChannel.onmessage = (event) => {
                // Handle messages from PiP window
                switch (event.data.type) {
                    case pipTimer.getMessageType('timerUpdate'):
                        this.update(event.data.state);
                        break;
                    case pipTimer.getMessageType('toggleTimer'):
                        if (this.dotNetRef) {
                            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onToggle')).catch(function(err) {
                                console.error('PiP toggle timer callback failed:', err);
                            });
                        }
                        break;
                    case pipTimer.getMessageType('resetTimer'):
                        if (this.dotNetRef) {
                            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onReset')).catch(function(err) {
                                console.error('PiP reset timer callback failed:', err);
                            });
                        }
                        break;
                    case pipTimer.getMessageType('switchSession'):
                        if (this.dotNetRef) {
                            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onSwitchSession'), event.data.sessionType).catch(function(err) {
                                console.error('PiP switch session callback failed:', err);
                            });
                        }
                        break;
                }
            };
        }
    },
    
    // Timer control functions (called from main window or .NET)
    toggleTimer: function() {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onToggle')).catch(function(err) {
                console.error('PiP toggle timer callback failed:', err);
            });
        }
    },
    
    resetTimer: function() {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onReset')).catch(function(err) {
                console.error('PiP reset timer callback failed:', err);
            });
        }
    },
    
    switchSession: function(sessionType) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onSwitchSession'), sessionType).catch(function(err) {
                console.error('PiP switch session callback failed:', err);
            });
        }
    },
    
    // Close PiP window
    close: function() {
        if (this.pipWindow && !this.pipWindow.closed) {
            this.pipWindow.close();
        }
        this.pipWindow = null;
        this.pipDocument = null;
        this.pipScriptInitialized = false; // Reset flag to allow re-initialization on next open
        
        if (this.broadcastChannel) {
            this.broadcastChannel.close();
            this.broadcastChannel = null;
        }
    },
    
    // Notify .NET that PiP was closed
    notifyClosed: function() {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onClosed')).catch(function(err) {
                console.error('PiP closed callback failed:', err);
            });
        }
    },
    
    // Helper methods to access constants (for cleaner code and testability)
    getBroadcastChannelName: function() {
        return pomodoroConstants.pip.broadcastChannel;
    },
    
    getMessageType: function(key) {
        return pomodoroConstants.pip.messages[key];
    },
    
    getCallbackName: function(key) {
        return pomodoroConstants.pip.callbacks[key];
    }
};

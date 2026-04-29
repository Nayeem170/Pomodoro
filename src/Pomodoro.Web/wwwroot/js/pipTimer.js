// Picture-in-Picture Timer for Pomodoro
// Provides a floating, always-on-top timer window

window.pipTimer = {
    pipWindow: null,
    pipDocument: null,
    broadcastChannel: null,
    dotNetRef: null,
    pipScriptInitialized: false,
    
    isSupported: function() {
        return pomodoroConstants.pip.apiName in window;
    },
    
    isOpen: function() {
        return this.pipWindow && !this.pipWindow.closed;
    },
    
    registerDotNetRef: function(dotNetRef) {
        this.dotNetRef = dotNetRef;
    },
    
    unregisterDotNetRef: function() {
        this.dotNetRef = null;
    },
    
    open: async function(timerState) {
        if (this.isOpen()) {
            this.close();
            return false;
        }
        
        var width = Math.round(Math.min(window.innerWidth, 420) * 0.9);
        var height = Math.round(width * 1.35 * 0.6) + 140;
        
        if (this.isSupported()) {
            try {
                this.pipWindow = await window[pomodoroConstants.pip.apiName].requestWindow({
                    width: width,
                    height: height
                });
                
                this.pipDocument = this.pipWindow.document;
                this.injectPipStyles();
                this.setupCloseHandler();
                this.renderTimer(timerState);
                this.setupBroadcastChannel();
                
                return true;
            } catch (e) {
                console.warn(pomodoroConstants.messages.pipFailed, e.message);
            }
        }
        
        return this.openFallback(timerState);
    },
    
    openFallback: function(timerState) {
        var width = Math.round(Math.min(window.innerWidth, 420) * 0.9);
        var height = Math.round(width * 1.35 * 0.6) + 140;
        
        const features = [
            'width=' + width,
            'height=' + height,
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
        
        return true;
    },
    
    getThemeClass: function(sessionType) {
        switch (sessionType) {
            case 0: return 'pomodoro-theme';
            case 1: return 'short-break-theme';
            case 2: return 'long-break-theme';
            default: return 'pomodoro-theme';
        }
    },
    
    injectPipStyles: function() {
        if (!this.pipDocument) return;
        
        var pipStyles = this.pipDocument.createElement('style');
        pipStyles.id = 'pip-styles';
        pipStyles.textContent = `
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body {
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                background: #1a1a2e;
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
                width: 100%;
                max-width: 380px;
            }
            .mode-tabs {
                display: flex;
                gap: 4px;
                background: #1f3460;
                border-radius: 12px;
                padding: 3px;
                margin: 10px 14px 8px;
            }
            .mode-tab {
                flex: 1;
                padding: 6px 0;
                font-size: 16px;
                text-align: center;
                border-radius: 8px;
                border: none;
                background: transparent;
                color: #a0a0a0;
                cursor: pointer;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                font-weight: 400;
                transition: background 0.15s;
            }
            .mode-tab:hover {
                background: #16213e;
            }
            .mode-tab.active {
                background: #16213e;
                color: #ffffff;
                font-weight: 500;
                border: 1px solid #2d4a6f;
            }
            .timer-card {
                background: #16213e;
                border: 1px solid #2d4a6f;
                border-radius: 12px;
                margin: 0 14px;
                overflow: hidden;
                transition: background 0.3s ease, border-color 0.3s ease;
            }
            .card-top {
                padding: 18px 14px 14px;
                text-align: center;
                display: flex;
                flex-direction: column;
                align-items: center;
            }
            .ring-area {
                position: relative;
                width: 180px;
                height: 180px;
                flex-shrink: 0;
            }
            .ring-area svg { transform: rotate(-90deg); }
            .ring-bg { fill: none; stroke: #1f3460; stroke-width: 10; }
            .ring-fill {
                fill: none;
                stroke: #e74c3c;
                stroke-width: 10;
                stroke-linecap: round;
                stroke-dasharray: 509;
                stroke-dashoffset: 0;
                transition: stroke-dashoffset 0.4s ease;
            }
            .ring-fill.short-break { stroke: #27ae60; }
            .ring-fill.long-break { stroke: #3498db; }
            .timer-center {
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                text-align: center;
                pointer-events: none;
            }
            .ttime {
                font-size: 42px;
                font-weight: 700;
                color: #ffffff;
                line-height: 1;
                font-family: 'Courier New', monospace;
                font-variant-numeric: tabular-nums;
            }
            .pip-container.running.pomodoro-theme .ttime { color: #e74c3c; }
            .pip-container.running.short-break-theme .ttime { color: #27ae60; }
            .pip-container.running.long-break-theme .ttime { color: #3498db; }
            .tmode {
                font-size: 15px;
                color: #6e7a8a;
                margin-top: 3px;
                letter-spacing: 0.06em;
            }
            .active-task {
                font-size: 16px;
                color: #a0a0a0;
                margin-top: 12px;
                margin-bottom: 10px;
                padding: 8px 12px;
                background: #1f3460;
                border-radius: 8px;
                text-align: left;
                display: flex;
                align-items: center;
                gap: 6px;
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: nowrap;
                width: 100%;
            }
            .task-dot {
                width: 6px;
                height: 6px;
                border-radius: 50%;
                background: #e74c3c;
                flex-shrink: 0;
            }
            .task-dot.short-break { background: #27ae60; }
            .task-dot.long-break { background: #3498db; }
            .ctrl-row {
                display: flex;
                align-items: center;
                justify-content: center;
                gap: 16px;
            }
            .ibtn {
                display: flex;
                align-items: center;
                justify-content: center;
                border-radius: 50%;
                cursor: pointer;
                border: 1px solid rgba(45, 74, 111, 0.6);
                background: transparent;
                color: #a0a0a0;
                padding: 0;
                transition: background 0.15s;
            }
            .ibtn.sm { width: 36px; height: 36px; font-size: 14px; }
            .ibtn.lg { width: 50px; height: 50px; border: none; color: white; font-size: 16px; }
            .ibtn.lg.pomodoro { background: #e74c3c; }
            .ibtn.lg.short-break { background: #27ae60; }
            .ibtn.lg.long-break { background: #3498db; }
            .ibtn.lg:hover { opacity: 0.9; }
            .ibtn.sm:hover { background: #1f3460; }
            .card-footer {
                display: flex;
                align-items: center;
                justify-content: space-between;
                padding: 8px 14px;
                border-top: 1px solid #2d4a6f;
                background: #1f3460;
            }
            .session-info {
                font-size: 16px;
                color: #6e7a8a;
            }
            .pip-container.pomodoro-theme .timer-card {
                background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, #16213e 100%);
                border: 1px solid rgba(231, 76, 60, 0.3);
            }
            .pip-container.short-break-theme .timer-card {
                background: linear-gradient(135deg, rgba(39, 174, 96, 0.15) 0%, #16213e 100%);
                border: 1px solid rgba(39, 174, 96, 0.3);
            }
            .pip-container.long-break-theme .timer-card {
                background: linear-gradient(135deg, rgba(52, 152, 219, 0.15) 0%, #16213e 100%);
                border: 1px solid rgba(52, 152, 219, 0.3);
            }
        `;
        this.pipDocument.head.appendChild(pipStyles);
    },
    
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
        this.injectPipStyles();
    },
    
    generateTimerHTML: function(state) {
        var sessionType = state.sessionType || 0;
        var sessionClass = pomodoroConstants.getSessionClass(sessionType);
        
        var remainingSeconds = state.remainingSeconds ?? 0;
        var minutes = Math.floor(remainingSeconds / 60);
        var seconds = remainingSeconds % 60;
        var timeDisplay = minutes.toString().padStart(2, '0') + ':' + seconds.toString().padStart(2, '0');
        
        var isRunning = state.isRunning || false;
        var isStarted = state.isStarted || false;
        var playPauseIcon = isRunning ? '\u23F8' : '\u25B6';
        var showReset = state.showReset || false;

        var circumference = 2 * Math.PI * 81;
        var totalSeconds = state.totalDurationSeconds || remainingSeconds;
        var progress = totalSeconds > 0 ? remainingSeconds / totalSeconds : 0;
        var dashOffset = circumference * (1 - progress);

        var modeLabel = sessionType === 0 ? 'FOCUSING' : sessionType === 1 ? 'SHORT BREAK' : 'LONG BREAK';
        
        var html = '<div class="mode-tabs">';
        html += '<button class="mode-tab ' + (sessionType === 0 ? 'active' : '') + '" onclick="window.pipSwitchSession(0)">Pomodoro</button>';
        html += '<button class="mode-tab ' + (sessionType === 1 ? 'active' : '') + '" onclick="window.pipSwitchSession(1)">Short break</button>';
        html += '<button class="mode-tab ' + (sessionType === 2 ? 'active' : '') + '" onclick="window.pipSwitchSession(2)">Long break</button>';
        html += '</div>';
        html += '<div class="timer-card">';
        html += '<div class="card-top">';
        html += '<div class="ring-area">';
        html += '<svg width="180" height="180" viewBox="0 0 180 180">';
        html += '<circle class="ring-bg" cx="90" cy="90" r="81"/>';
        html += '<circle class="ring-fill ' + sessionClass + '" cx="90" cy="90" r="81" style="stroke-dasharray: ' + circumference.toFixed(1) + '; stroke-dashoffset: ' + dashOffset.toFixed(1) + '"/>';
        html += '</svg>';
        html += '<div class="timer-center">';
        html += '<div class="ttime">' + timeDisplay + '</div>';
        html += '<div class="tmode">' + modeLabel + '</div>';
        html += '</div></div>';
        if (state.taskName) {
            html += '<div class="active-task"><div class="task-dot ' + sessionClass + '"></div><span>' + state.taskName + '</span></div>';
        }
        html += '<div class="ctrl-row">';
        html += '<button class="ibtn lg ' + sessionClass + '" onclick="window.pipToggleTimer()">' + playPauseIcon + '</button>';
        if (showReset) {
            html += '<button class="ibtn sm" onclick="window.pipResetTimer()">&#x21BA;</button>';
        }
        html += '</div></div>';
        html += '<div class="card-footer">';
        html += '<span class="session-info">' + modeLabel + '</span>';
        html += '<span class="session-info">' + timeDisplay + '</span>';
        html += '</div></div>';
        
        return html;
    },
    
    renderTimer: function(timerState) {
        if (!this.pipDocument) return;
        
        var container = this.pipDocument.getElementById('pip-container');
        if (!container) {
            container = this.pipDocument.createElement('div');
            container.id = 'pip-container';
            this.pipDocument.body.appendChild(container);
        }
        
        if (!this.pipScriptInitialized) {
            this.ensurePipScript();
            this.pipScriptInitialized = true;
        }
        
        var themeClass = this.getThemeClass(timerState.sessionType || 0);
        var runningClass = timerState.isRunning ? 'running' : '';
        container.className = 'pip-container ' + themeClass + ' ' + runningClass;
        container.innerHTML = this.generateTimerHTML(timerState);
        
        if (this.pipDocument.body) {
            this.pipDocument.body.setAttribute('tabindex', '0');
            this.pipDocument.body.focus();
        }
    },
    
    ensurePipScript: function() {
        if (!this.pipDocument) return;
        
        var existingScript = this.pipDocument.getElementById('pip-control-script');
        if (existingScript) {
            return;
        }
        
        var bcName = pipTimer.getBroadcastChannelName();
        var msgToggleTimer = pipTimer.getMessageType('toggleTimer');
        var msgResetTimer = pipTimer.getMessageType('resetTimer');
        var msgSwitchSession = pipTimer.getMessageType('switchSession');
        var msgTimerUpdate = pipTimer.getMessageType('timerUpdate');
        
        var pipScript = this.pipDocument.createElement('script');
        pipScript.id = 'pip-control-script';
        pipScript.textContent = [
            '(function() {',
            '    var bc = new BroadcastChannel("' + bcName + '");',
            '    window.pipToggleTimer = function() { bc.postMessage({ type: "' + msgToggleTimer + '" }); };',
            '    window.pipResetTimer = function() { bc.postMessage({ type: "' + msgResetTimer + '" }); };',
            '    window.pipSwitchSession = function(sessionType) { bc.postMessage({ type: "' + msgSwitchSession + '", sessionType: sessionType }); };',
            '    bc.onmessage = function(event) {',
            '        if (event.data.type === "' + msgTimerUpdate + '") {}',
            '    };',
            '    window.addEventListener("keydown", function(event) {',
            '        if (event.target.tagName === "INPUT" || event.target.tagName === "TEXTAREA") return;',
            '        var key = event.key.toLowerCase();',
            '        if (key === " " || event.code === "Space") { event.preventDefault(); event.stopPropagation(); window.pipToggleTimer(); }',
            '        else if (key === "r") { event.preventDefault(); event.stopPropagation(); window.pipResetTimer(); }',
            '        else if (key === "p") { event.preventDefault(); event.stopPropagation(); window.pipSwitchSession(0); }',
            '        else if (key === "s") { event.preventDefault(); event.stopPropagation(); window.pipSwitchSession(1); }',
            '        else if (key === "l") { event.preventDefault(); event.stopPropagation(); window.pipSwitchSession(2); }',
            '    }, { capture: true });',
            '    document.body.setAttribute("tabindex", "0");',
            '    document.body.focus();',
            '})();'
        ].join('\n');
        this.pipDocument.head.appendChild(pipScript);
    },
    
    update: function(timerState) {
        if (!this.isOpen()) return;
        this.renderTimer(timerState);
    },
    
    setupCloseHandler: function() {
        if (!this.pipWindow) return;
        
        this.pipWindow.addEventListener('pagehide', function() {
            pipTimer.pipWindow = null;
            pipTimer.pipDocument = null;
            pipTimer.pipScriptInitialized = false;
            pipTimer.notifyClosed();
        });
    },
    
    setupBroadcastChannel: function() {
        if ('BroadcastChannel' in window) {
            this.broadcastChannel = new BroadcastChannel(pomodoroConstants.pip.broadcastChannel);
            this.broadcastChannel.onmessage = function(event) {
                switch (event.data.type) {
                    case pipTimer.getMessageType('timerUpdate'):
                        pipTimer.update(event.data.state);
                        break;
                    case pipTimer.getMessageType('toggleTimer'):
                        if (pipTimer.dotNetRef) {
                            pipTimer.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onToggle')).catch(function(err) {
                                console.error('PiP toggle timer callback failed:', err);
                            });
                        }
                        break;
                    case pipTimer.getMessageType('resetTimer'):
                        if (pipTimer.dotNetRef) {
                            pipTimer.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onReset')).catch(function(err) {
                                console.error('PiP reset timer callback failed:', err);
                            });
                        }
                        break;
                    case pipTimer.getMessageType('switchSession'):
                        if (pipTimer.dotNetRef) {
                            pipTimer.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onSwitchSession'), event.data.sessionType).catch(function(err) {
                                console.error('PiP switch session callback failed:', err);
                            });
                        }
                        break;
                }
            };
        }
    },
    
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
    
    close: function() {
        if (this.pipWindow && !this.pipWindow.closed) {
            this.pipWindow.close();
        }
        this.pipWindow = null;
        this.pipDocument = null;
        this.pipScriptInitialized = false;
        
        if (this.broadcastChannel) {
            this.broadcastChannel.close();
            this.broadcastChannel = null;
        }
    },
    
    notifyClosed: function() {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(pipTimer.getCallbackName('onClosed')).catch(function(err) {
                console.error('PiP closed callback failed:', err);
            });
        }
    },
    
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

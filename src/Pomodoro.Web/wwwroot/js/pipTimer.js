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
        
        var width = 380;
        var height = 430;
        
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
        var width = 380;
        var height = 430;
        
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
        var container = this.pipDocument.getElementById('pip-container');
        if (container) {
            var themeClass = this.getThemeClass(timerState.sessionType || 0);
            var runningClass = timerState.isRunning ? 'running' : '';
            container.className = 'pip-container ' + themeClass + ' ' + runningClass;
        }
        if (!this.pipScriptInitialized) {
            this.ensurePipScript();
            this.pipScriptInitialized = true;
        }
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

    escapeHtml: function(text) {
        if (!text) return '';
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },
    
    injectPipStyles: function() {
        if (!this.pipDocument) return;
        
        var pipStyles = this.pipDocument.createElement('style');
        pipStyles.id = 'pip-styles';
        pipStyles.textContent = `
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body {
                font-family: system-ui, -apple-system, sans-serif;
                background: #162032;
                color: #ffffff;
                min-height: 100vh;
                display: flex;
                flex-direction: column;
                overflow: hidden;
                padding: 0;
            }
            .pip-container {
                width: 100%;
                display: flex;
                flex-direction: column;
                flex: 1;
            }
            .pip-tabs {
                display: flex;
                padding: 6px 10px 0;
                gap: 2px;
                background: #162032;
            }
            .pip-tab {
                flex: 1;
                padding: 7px 0;
                border-radius: 7px 7px 0 0;
                border: none;
                background: transparent;
                font-size: 14px;
                font-weight: 400;
                color: rgba(255,255,255,0.3);
                cursor: pointer;
                font-family: system-ui, -apple-system, sans-serif;
                border-bottom: 2px solid transparent;
            }
            .pip-tab.act {
                color: #e8edf8;
                font-weight: 600;
            }
            .pip-tab.act.pomodoro { border-bottom-color: #e74c3c; }
            .pip-tab.act.short-break { border-bottom-color: #27ae60; }
            .pip-tab.act.long-break { border-bottom-color: #3498db; }
            .pip-timer-area {
                display: flex;
                flex-direction: column;
                align-items: center;
                padding: 12px 20px 10px;
                gap: 10px;
            }
            .pip-container.pomodoro-theme .pip-timer-area {
                background: linear-gradient(180deg, rgba(231,76,60,.12), #162032);
            }
            .pip-container.short-break-theme .pip-timer-area {
                background: linear-gradient(180deg, rgba(39,174,96,.12), #162032);
            }
            .pip-container.long-break-theme .pip-timer-area {
                background: linear-gradient(180deg, rgba(52,152,219,.12), #162032);
            }
            .ring-wrap { position: relative; }
            .ring-wrap svg { display: block; transform: rotate(-90deg); }
            .ring-bg { fill: none; stroke: #1e2a40; stroke-width: 14; }
            .ring-fill {
                fill: none;
                stroke: #e74c3c;
                stroke-width: 14;
                stroke-linecap: round;
                stroke-dasharray: 553;
                stroke-dashoffset: 0;
                transition: stroke-dashoffset 0.4s ease;
            }
            .ring-fill.short-break { stroke: #27ae60; }
            .ring-fill.long-break { stroke: #3498db; }
            .ring-center {
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                text-align: center;
                pointer-events: none;
            }
            .ring-time {
                font-size: 42px;
                font-weight: 700;
                color: #e8edf8;
                letter-spacing: -1px;
                line-height: 1;
                font-family: 'Courier New', 'Lucida Console', monospace;
                font-variant-numeric: tabular-nums;
            }
            .pip-container.running.pomodoro-theme .ring-time { color: #e74c3c; }
            .pip-container.running.short-break-theme .ring-time { color: #27ae60; }
            .pip-container.running.long-break-theme .ring-time { color: #3498db; }
            .ring-label {
                font-size: 15px;
                color: #8a97b8;
                letter-spacing: .1em;
                margin-top: 5px;
            }
            .pip-task {
                width: 100%;
                background: #1e2a40;
                border-radius: 8px;
                padding: 8px 12px;
                display: flex;
                align-items: center;
                gap: 8px;
            }
            .pip-task-dot {
                width: 7px;
                height: 7px;
                border-radius: 50%;
                background: #e74c3c;
                flex-shrink: 0;
            }
            .pip-task-dot.short-break { background: #27ae60; }
            .pip-task-dot.long-break { background: #3498db; }
            .pip-task-name {
                font-size: 13px;
                color: #e8edf8;
                flex: 1;
                max-width: 100%;
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: nowrap;
            }
            .pip-ctrl {
                display: flex;
                align-items: center;
                justify-content: center;
                gap: 16px;
            }
            .pip-play {
                width: 52px;
                height: 52px;
                border-radius: 50%;
                background: #e74c3c;
                border: none;
                display: flex;
                align-items: center;
                justify-content: center;
                cursor: pointer;
                box-shadow: 0 4px 18px rgba(231,76,60,.45);
            }
            .pip-play.short-break {
                background: #27ae60;
                box-shadow: 0 4px 18px rgba(39,174,96,.45);
            }
            .pip-play.long-break {
                background: #3498db;
                box-shadow: 0 4px 18px rgba(52,152,219,.45);
            }
            .pip-play:hover {
                opacity: 0.9;
            }
            .pip-play:disabled {
                opacity: 0.4;
                cursor: not-allowed;
            }
            .pip-reset {
                width: 36px;
                height: 36px;
                border-radius: 50%;
                border: 0.5px solid rgba(255,255,255,0.14);
                background: transparent;
                display: flex;
                align-items: center;
                justify-content: center;
                cursor: pointer;
                color: #8a97b8;
            }
            .pip-footer {
                display: flex;
                align-items: center;
                justify-content: space-between;
                padding: 10px 20px 14px;
                background: #162032;
                margin-top: auto;
            }
            .pip-footer span { font-size: 14px; }
            .pip-footer .lbl { color: #6e7a8a; }
            .pip-footer .val { color: #a0aec0; font-weight: 600; }
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

        var circumference = 2 * Math.PI * 88;
        var totalSeconds = state.totalDurationSeconds || remainingSeconds;
        var progress = totalSeconds > 0 ? remainingSeconds / totalSeconds : 0;
        var dashOffset = circumference * (1 - progress);

        var modeLabel = sessionType === 0 ? 'FOCUSING' : sessionType === 1 ? 'SHORT BREAK' : 'LONG BREAK';
        var isRunning = state.isRunning || false;
        var isStarted = state.isStarted || false;
        var canStart = state.canStart !== false;
        var taskName = state.taskName || null;
        var endsAt = state.endsAt || null;

        var playIcon = '<svg width="20" height="20" viewBox="0 0 20 20" fill="none"><path d="M6 4l10 6-10 6V4z" fill="white"/></svg>';
        var pauseIcon = '<svg width="18" height="18" viewBox="0 0 18 18" fill="none"><rect x="3" y="2" width="4" height="14" rx="1" fill="white"/><rect x="11" y="2" width="4" height="14" rx="1" fill="white"/></svg>';
        var resetIcon = '<svg width="15" height="15" viewBox="0 0 15 15" fill="none"><path d="M2.5 7.5A5 5 0 1 0 4.2 4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/><path d="M2.5 2v2.5H5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>';

        var html = '<div class="pip-tabs">';
        html += '<button class="pip-tab ' + (sessionType === 0 ? 'act pomodoro' : '') + '" data-action="switch" data-session="0">Pomodoro</button>';
        html += '<button class="pip-tab ' + (sessionType === 1 ? 'act short-break' : '') + '" data-action="switch" data-session="1">Short break</button>';
        html += '<button class="pip-tab ' + (sessionType === 2 ? 'act long-break' : '') + '" data-action="switch" data-session="2">Long break</button>';
        html += '</div>';
        html += '<div class="pip-timer-area">';
        html += '<div class="ring-wrap">';
        html += '<svg width="220" height="220" viewBox="0 0 220 220">';
        html += '<circle class="ring-bg" cx="110" cy="110" r="88"/>';
        html += '<circle class="ring-fill ' + sessionClass + '" cx="110" cy="110" r="88" style="stroke-dasharray: ' + circumference.toFixed(1) + '; stroke-dashoffset: ' + dashOffset.toFixed(1) + '"/>';
        html += '</svg>';
        html += '<div class="ring-center">';
        html += '<div class="ring-time">' + timeDisplay + '</div>';
        html += '<div class="ring-label">' + modeLabel + '</div>';
        html += '</div></div>';

        if (sessionType === 0 && taskName) {
            html += '<div class="pip-task">';
            html += '<div class="pip-task-dot ' + sessionClass + '"></div>';
            html += '<span class="pip-task-name">' + this.escapeHtml(taskName) + '</span>';
            html += '</div>';
        }

        if (!isStarted && !isRunning) {
            var startDisabled = sessionType === 0 && !canStart;
            var startTitle = startDisabled ? 'Select a task first' : 'Start';
            html += '<div class="pip-ctrl">';
            html += '<button class="pip-play ' + sessionClass + (startDisabled ? '" disabled' : '"') + ' data-action="toggle" aria-label="' + startTitle + '" title="' + startTitle + '">';
            html += playIcon;
            html += '</button>';
            html += '</div>';
        } else if (isRunning) {
            html += '<div class="pip-ctrl">';
            html += '<button class="pip-reset" data-action="reset" aria-label="Reset timer">';
            html += resetIcon;
            html += '</button>';
            html += '<button class="pip-play ' + sessionClass + '" data-action="toggle" aria-label="Pause">';
            html += pauseIcon;
            html += '</button>';
            html += '<div style="width:36px"></div>';
            html += '</div>';
        } else {
            html += '<div class="pip-ctrl">';
            html += '<button class="pip-reset" data-action="reset" aria-label="Reset timer">';
            html += resetIcon;
            html += '</button>';
            html += '<button class="pip-play ' + sessionClass + '" data-action="toggle" aria-label="Resume">';
            html += playIcon;
            html += '</button>';
            html += '<div style="width:36px"></div>';
            html += '</div>';
        }
        html += '</div>';

        var durationMinutes = Math.round((state.totalDurationSeconds || 0) / 60);
        var durationLabel = sessionType === 0 ? durationMinutes + ' min session'
            : durationMinutes + ' min break';
        var footerLeft = '';
        var footerRight = '';
        if (!isStarted && !isRunning) {
            footerLeft = durationLabel;
        } else if (isRunning && endsAt) {
            footerLeft = 'Ends at';
            footerRight = endsAt;
        } else if (isStarted && !isRunning) {
            if (endsAt) {
                footerLeft = 'Paused · ends at';
                footerRight = endsAt;
            } else {
                footerLeft = durationLabel;
            }
        }
        html += '<div class="pip-footer">';
        html += '<span class="lbl">' + footerLeft + '</span>';
        html += '<span class="val">' + footerRight + '</span>';
        html += '</div>';

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
            '    function toggle() { bc.postMessage({ type: "' + msgToggleTimer + '" }); }',
            '    function reset() { bc.postMessage({ type: "' + msgResetTimer + '" }); }',
            '    function switchSession(t) { bc.postMessage({ type: "' + msgSwitchSession + '", sessionType: t }); }',
            '    document.addEventListener("click", function(e) {',
            '        var el = e.target.closest("[data-action]");',
            '        if (!el) return;',
            '        var action = el.getAttribute("data-action");',
            '        if (action === "toggle") toggle();',
            '        else if (action === "reset") reset();',
            '        else if (action === "switch") switchSession(parseInt(el.getAttribute("data-session"), 10));',
            '    });',
            '    window.addEventListener("keydown", function(event) {',
            '        if (event.target.tagName === "INPUT" || event.target.tagName === "TEXTAREA") return;',
            '        var key = event.key.toLowerCase();',
            '        if (key === " " || key === "spacebar") { event.preventDefault(); event.stopPropagation(); toggle(); }',
            '        else if (key === "r") { event.preventDefault(); event.stopPropagation(); reset(); }',
            '        else if (key === "p") { event.preventDefault(); event.stopPropagation(); switchSession(0); }',
            '        else if (key === "s") { event.preventDefault(); event.stopPropagation(); switchSession(1); }',
            '        else if (key === "l") { event.preventDefault(); event.stopPropagation(); switchSession(2); }',
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

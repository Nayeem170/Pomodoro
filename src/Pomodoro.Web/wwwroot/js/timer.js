// Timer module for Blazor WebAssembly
// Uses Web Worker to avoid browser throttling in background tabs
window.timerFunctions = {
    worker: null,
    dotNetRef: null,
    _visibilityHandler: null,

    start: function (dotNetReference) {
        this.stop();

        this.dotNetRef = dotNetReference;

        var workerCode = [
            'let intervalId = null;',
            'self.onmessage = function(e) {',
            '    if (e.data.command === "start") {',
            '        if (intervalId) clearInterval(intervalId);',
            '        intervalId = setInterval(function() {',
            '            self.postMessage({ type: "tick" });',
            '        }, 1000);',
            '    } else if (e.data.command === "stop") {',
            '        if (intervalId) {',
            '            clearInterval(intervalId);',
            '            intervalId = null;',
            '        }',
            '    }',
            '};'
        ].join('\n');

        var blob = new Blob([workerCode], { type: 'application/javascript' });
        this.worker = new Worker(URL.createObjectURL(blob));

        var self = this;
        this.worker.onmessage = function(e) {
            if (e.data.type === 'tick') {
                var ref = self.dotNetRef;
                if (ref) {
                    try {
                        ref.invokeMethodAsync(pomodoroConstants.jsCallbacks.onTimerTick);
                    } catch (err) {
                        console.error(pomodoroConstants.messages.timerTickError + err);
                        self.stop();
                    }
                }
            }
        };

        this.worker.postMessage({ command: 'start' });

        this._visibilityHandler = function() {
            if (!document.hidden) {
                var ref = self.dotNetRef;
                if (ref && self.worker) {
                    try {
                        ref.invokeMethodAsync(pomodoroConstants.jsCallbacks.onTimerTick);
                    } catch (err) {
                        console.error(pomodoroConstants.messages.timerTickError + err);
                    }
                }
            }
        };
        document.addEventListener('visibilitychange', this._visibilityHandler);
    },

    stop: function () {
        if (this.worker) {
            this.worker.postMessage({ command: 'stop' });
            this.worker.terminate();
            this.worker = null;
        }
        if (this._visibilityHandler) {
            document.removeEventListener('visibilitychange', this._visibilityHandler);
            this._visibilityHandler = null;
        }
        this.dotNetRef = null;
    },

    isRunning: function () {
        return this.worker !== null;
    }
};

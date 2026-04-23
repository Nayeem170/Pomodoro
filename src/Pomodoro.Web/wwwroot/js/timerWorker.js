// Web Worker for Pomodoro timer
// Runs setInterval in a worker thread - NOT throttled by browsers in background tabs
let intervalId = null;

self.onmessage = function(e) {
    if (e.data.command === 'start') {
        if (intervalId) clearInterval(intervalId);
        intervalId = setInterval(function() {
            self.postMessage({ type: 'tick' });
        }, 1000);
    } else if (e.data.command === 'stop') {
        if (intervalId) {
            clearInterval(intervalId);
            intervalId = null;
        }
    }
};

// Timer module for Blazor WebAssembly
window.timerFunctions = {
    intervalId: null,
    dotNetRef: null,
    
    start: function (dotNetReference) {
        // Stop any existing timer first to prevent memory leaks
        this.stop();
        
        // Store reference and start new interval
        this.dotNetRef = dotNetReference;
        
        var self = this;
        this.intervalId = setInterval(function() {
            // Capture reference locally to prevent race condition with stop()
            var ref = self.dotNetRef;
            if (ref) {
                try {
                    ref.invokeMethodAsync(pomodoroConstants.jsCallbacks.onTimerTick);
                } catch (e) {
                    console.error(pomodoroConstants.messages.timerTickError + e);
                    // Stop the timer on error to prevent repeated failures
                    self.stop();
                }
            }
        }, pomodoroConstants.timerIntervalMs);
    },
    
    stop: function () {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
        // Clear the dotNetRef to prevent stale callbacks
        this.dotNetRef = null;
    },
    
    isRunning: function () {
        return this.intervalId !== null;
    }
};

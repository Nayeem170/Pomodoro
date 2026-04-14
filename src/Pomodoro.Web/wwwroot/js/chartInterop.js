// Chart.js interop functions for Blazor
// Uses pomodoroConstants for configurable values
// Implements lazy loading for Chart.js to improve initial page load
function getBarThickness() {
    const chartStyling = window.pomodoroConstants.chartStyling;
    const screenWidth = window.innerWidth;
    if (screenWidth <= 480) return chartStyling.barThicknessSmall;
    if (screenWidth <= 768) return chartStyling.barThicknessMedium;
    return chartStyling.barThickness;
}

window.chartInterop = {
    charts: {},
    chartJsLoaded: false,
    chartJsLoading: false,
    chartJsLoadPromise: null,
    
    /// Lazy loads Chart.js library on first use
    /// Returns a promise that resolves when Chart.js is ready
    ///
    /// RACE CONDITION HANDLING:
    /// If multiple chart creation methods (createBarChart, createGroupedBarChart, createDoughnutChart)
    /// are called simultaneously before Chart.js finishes loading, they all await the same promise.
    /// This is handled by storing the promise in chartJsLoadPromise and returning it for concurrent callers,
    /// ensuring Chart.js is only loaded once regardless of how many charts are requested simultaneously.
    ensureChartJsLoaded: async function() {
        // Already loaded
        if (this.chartJsLoaded && typeof Chart !== 'undefined') {
            return true;
        }
        
        // Already loading - return existing promise to prevent duplicate script injection
        if (this.chartJsLoading && this.chartJsLoadPromise) {
            return this.chartJsLoadPromise;
        }
        
        // Start loading
        this.chartJsLoading = true;
        this.chartJsLoadPromise = new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = 'lib/chart.js/dist/chart.umd.min.js';
            script.onload = () => {
                this.chartJsLoaded = true;
                this.chartJsLoading = false;
                resolve(true);
            };
            script.onerror = (error) => {
                this.chartJsLoading = false;
                console.error('Failed to load Chart.js:', error);
                reject(error);
            };
            document.head.appendChild(script);
        });
        
        return this.chartJsLoadPromise;
    },
    
    ensureInitialized: function() {
        // Chart.js should be loaded via lazy loading
        if (typeof Chart === 'undefined') {
            console.warn(window.pomodoroConstants.messages.chartNotLoaded);
        }
    },
    
    createBarChart: async function(canvasId, labels, data, label, highlightIndex) {
        // Lazy load Chart.js if not already loaded
        await this.ensureChartJsLoaded();
        
        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(window.pomodoroConstants.messages.canvasNotFound, canvasId);
            return;
        }
        
        // Destroy existing chart if it exists
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        
        // Get constants reference
        const constants = window.pomodoroConstants;
        const chartColors = constants.chartColors;
        const chartStyling = constants.chartStyling;
        
        const barThickness = getBarThickness();
        const backgroundColors = data.map((_, index) =>
            index === highlightIndex ? chartColors.highlightBar : chartColors.defaultBar
        );
        
        // Create new chart
        this.charts[canvasId] = new Chart(ctx, {
            type: window.pomodoroConstants.chartTypes.bar,
            data: {
                labels: labels,
                datasets: [{
                    label: label || window.pomodoroConstants.activityLabels.pomodoros,
                    data: data,
                    backgroundColor: backgroundColors,
                    borderColor: '#ffffff',
                    borderWidth: 2,
                    borderRadius: chartStyling.borderRadius,
                    barThickness: barThickness
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                // Disable animations for better performance
                animation: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: chartColors.tooltipBackground,
                        titleColor: chartColors.white,
                        bodyColor: chartColors.white,
                        padding: chartStyling.tooltipPadding,
                        cornerRadius: chartStyling.tooltipCornerRadius,
                        callbacks: {
                            label: function(context) {
                                const pomodoroCount = context.parsed.y;
                                const minutes = constants.calculateFocusTime(pomodoroCount);
                                const timeText = constants.formatTime(minutes);
                                return `${pomodoroCount} pomodoro${pomodoroCount !== 1 ? 's' : ''} (${timeText})`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: chartColors.tickColor
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: chartColors.gridColor
                        },
                        ticks: {
                            color: chartColors.tickColor,
                            stepSize: 1,
                            callback: function(value) {
                                // Convert pomodoro count to time using user's settings
                                const minutes = constants.calculateFocusTime(value);
                                return constants.formatTime(minutes);
                            }
                        }
                    }
                }
            }
        });
    },
    
    updateChart: function(canvasId, data) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].data.datasets[0].data = data;
            // Disable animation on update for better performance
            this.charts[canvasId].update('none');
        }
    },
    
    createGroupedBarChart: async function(canvasId, labels, focusData, breakData, highlightIndex) {
        // Lazy load Chart.js if not already loaded
        await this.ensureChartJsLoaded();
        
        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(window.pomodoroConstants.messages.canvasNotFound, canvasId);
            return;
        }
        
        // Destroy existing chart if it exists
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        
        const constants = window.pomodoroConstants;
        const chartColors = constants.chartColors;
        const chartStyling = constants.chartStyling;

        // White border for all bars
        const whiteBorder = 'rgba(255, 255, 255, 1)';

        // Focus Time: Red (higher opacity for better visibility)
        const focusNormalBg = 'rgba(239, 68, 68, 0.7)';     // Red-500 at 70% opacity
        const focusHighlightBg = 'rgba(239, 68, 68, 1)';    // Red-500 at 100% opacity

        // Break Time: Green (higher opacity for better visibility)
        const breakNormalBg = 'rgba(34, 197, 94, 0.7)';     // Green-500 at 70% opacity
        const breakHighlightBg = 'rgba(34, 197, 94, 1)';    // Green-500 at 100% opacity

        // Ensure focusData and breakData are arrays before mapping
        const focusDataArray = Array.isArray(focusData) ? focusData : [];
        const breakDataArray = Array.isArray(breakData) ? breakData : [];

        // Generate colors: all bars have white border (thick), highlighted bars have full opacity
        const focusBackgroundColors = focusDataArray.map((_, index) =>
            index === highlightIndex ? focusHighlightBg : focusNormalBg
        );
        const focusBorderColors = focusDataArray.map(() => whiteBorder);
        const focusBorderWidths = focusDataArray.map(() => 1);

        const breakBackgroundColors = breakDataArray.map((_, index) =>
            index === highlightIndex ? breakHighlightBg : breakNormalBg
        );
        const breakBorderColors = breakDataArray.map(() => whiteBorder);
        const breakBorderWidths = breakDataArray.map(() => 1);

        const barThickness = getBarThickness();
        this.charts[canvasId] = new Chart(ctx, {
            type: window.pomodoroConstants.chartTypes.bar,
            data: {
                labels: labels,
                datasets: [
                    {
                        label: window.pomodoroConstants.activityLabels.focusTimeLabel,
                        data: focusDataArray,
                        backgroundColor: focusBackgroundColors,
                        borderColor: focusBorderColors,
                        borderWidth: focusBorderWidths,
                        borderRadius: chartStyling.borderRadius,
                        barThickness: barThickness
                    },
                    {
                        label: window.pomodoroConstants.activityLabels.breakTimeLabel,
                        data: breakData,
                        backgroundColor: breakBackgroundColors,
                        borderColor: breakBorderColors,
                        borderWidth: breakBorderWidths,
                        borderRadius: chartStyling.borderRadius,
                        barThickness: barThickness
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                // Disable animations for better performance
                animation: false,
                plugins: {
                    legend: {
                        display: true,
                        position: constants.chartPositions.legendTop,
                        labels: {
                            color: chartColors.white,
                            padding: chartStyling.legendPadding || 15,
                            usePointStyle: true,
                            pointStyle: constants.chartPositions.pointStyleRectRounded,
                            font: {
                                size: 13,
                                weight: '600'
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: chartColors.tooltipBackground,
                        titleColor: chartColors.white,
                        bodyColor: chartColors.white,
                        padding: chartStyling.tooltipPadding,
                        cornerRadius: chartStyling.tooltipCornerRadius,
                        callbacks: {
                            label: function(context) {
                                const minutes = context.parsed.y;
                                return `${context.dataset.label}: ${constants.formatTime(minutes)}`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: chartColors.tickColor }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: chartColors.gridColor },
                        ticks: {
                            color: chartColors.tickColor,
                            callback: function(value) {
                                return constants.formatTime(value);
                            }
                        }
                    }
                }
            }
        });
    },
    
    destroyChart: function(canvasId) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
            delete this.charts[canvasId];
        }
    },
    
    createDoughnutChart: async function(canvasId, labels, data, centerText) {
        // Lazy load Chart.js if not already loaded
        await this.ensureChartJsLoaded();
        
        const msg = window.pomodoroConstants.messages;
        
        // Use requestAnimationFrame to ensure DOM is ready
        requestAnimationFrame(() => {
            this._createDoughnutChartInternal(canvasId, labels, data, centerText);
        });
    },
    
    _createDoughnutChartInternal: function(canvasId, labels, data, centerText) {
        const msg = window.pomodoroConstants.messages;
        const ctx = document.getElementById(canvasId);
        if (!ctx) {
            console.error(msg.chartInteropCanvasNotFound, canvasId);
            return;
        }
        
        // Get computed dimensions
        const rect = ctx.getBoundingClientRect();
        
        // Check if Chart.js is loaded
        if (typeof Chart === 'undefined') {
            console.error(msg.chartInteropNotLoaded);
            return;
        }
        
        // Destroy existing chart if it exists
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }
        
        // Get constants reference
        const constants = window.pomodoroConstants;
        const chartStyling = constants.chartStyling;
        const chartColors = constants.chartColors;
        const doughnutColors = constants.doughnutColors;
        
        // Get activity labels from constants
        const activityLabels = constants.activityLabels;
        
        // Assign colors based on label names
        const backgroundColors = labels.map((label, index) => {
            if (label === activityLabels.shortBreaks) return doughnutColors.green;
            if (label === activityLabels.longBreaks) return doughnutColors.purple;
            return doughnutColors.backgrounds[index % doughnutColors.backgrounds.length];
        });
        
        const borderColorValues = labels.map((label, index) => {
            if (label === activityLabels.shortBreaks) return doughnutColors.greenBorder;
            if (label === activityLabels.longBreaks) return doughnutColors.purpleBorder;
            return doughnutColors.backgrounds[index % doughnutColors.backgrounds.length];
        });
        
        // Create new doughnut chart
        this.charts[canvasId] = new Chart(ctx, {
            type: window.pomodoroConstants.chartTypes.doughnut,
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColors,
                    borderColor: borderColorValues,
                    borderWidth: chartStyling.doughnutBorderWidth,
                    hoverOffset: chartStyling.doughnutHoverOffset
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: chartStyling.doughnutCutout,
                // Disable animations for better performance
                animation: false,
                plugins: {
                    legend: {
                        position: window.pomodoroConstants.chartPositions.legendRight,
                        align: window.pomodoroConstants.chartPositions.legendAlignStart,
                        labels: {
                            color: chartColors.white,
                            padding: chartStyling.legendPadding,
                            usePointStyle: true,
                            pointStyle: window.pomodoroConstants.chartPositions.cutoutCircle,
                            fontColor: chartColors.white,
                            textAlign: window.pomodoroConstants.chartPositions.textAlignLeft,
                            font: {
                                size: 13,
                                weight: '600'
                            },
                            generateLabels: function(chart) {
                                const data = chart.data;
                                const total = data.datasets[0].data.reduce((a, b) => a + b, 0);
                                return data.labels.map((label, i) => {
                                    const value = data.datasets[0].data[i];
                                    const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
                                    // Format time using constants
                                    const timeText = window.pomodoroConstants.formatTime(value);
                                    return {
                                        text: `${label}: ${timeText} (${percentage}%)`,
                                        fillStyle: data.datasets[0].backgroundColor[i],
                                        strokeStyle: data.datasets[0].borderColor[i],
                                        lineWidth: 2,
                                        hidden: false,
                                        index: i,
                                        fontColor: chartColors.white
                                    };
                                });
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: chartColors.tooltipBackground,
                        titleColor: chartColors.white,
                        bodyColor: chartColors.white,
                        padding: chartStyling.doughnutTooltipPadding,
                        cornerRadius: chartStyling.tooltipCornerRadius,
                        callbacks: {
                            label: function(context) {
                                const value = context.parsed;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = Math.round((value / total) * 100);
                                return `${context.label}: ${value} min (${percentage}%)`;
                            }
                        }
                    }
                }
            },
            plugins: [{
                id: constants.chartPlugins.centerText,
                beforeDraw: function(chart) {
                    if (centerText) {
                        const ctx = chart.ctx;
                        const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
                        const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;
                        
                        ctx.save();
                        ctx.font = chartStyling.centerTextFont;
                        ctx.fillStyle = chartColors.legendLabel;
                        ctx.textAlign = constants.chartPositions.textAlignCenter;
                        ctx.textBaseline = constants.chartPositions.textBaselineMiddle;
                        ctx.fillText(centerText, centerX, centerY);
                        ctx.restore();
                    }
                }
            }]
        });
    }
};

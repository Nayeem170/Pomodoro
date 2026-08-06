namespace Pomodoro.Web;

public static partial class Constants
{
    public static class Charts
    {
        public const string TimeDistributionCanvasId = "timeDistributionChart";
        public const string PomodorosLabel = "Pomodoros";
        public const string ShortBreaksLabel = "Short Breaks";
        public const string LongBreaksLabel = "Long Breaks";

        // Weekly chart constants
        public const string WeeklyChartPrefix = "weekly-chart-";

        // Day indices for weekly chart
        public const int DaysPerWeek = 7;

    }

    public static class ChartJsFunctions
    {
        /// <summary>Creates a bar chart</summary>
        public const string CreateBarChart = "chartInterop.createBarChart";

        /// <summary>Creates a grouped bar chart with multiple datasets</summary>
        public const string CreateGroupedBarChart = "chartInterop.createGroupedBarChart";

        /// <summary>Creates a doughnut chart</summary>
        public const string CreateDoughnutChart = "chartInterop.createDoughnutChart";

        /// <summary>Updates an existing chart</summary>
        public const string UpdateChart = "chartInterop.updateChart";

        /// <summary>Destroys a chart</summary>
        public const string DestroyChart = "chartInterop.destroyChart";

        /// <summary>Ensures chart library is initialized</summary>
        public const string EnsureInitialized = "chartInterop.ensureInitialized";
    }
}

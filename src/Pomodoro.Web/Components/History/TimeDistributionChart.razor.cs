using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

public class TimeDistributionChartBase : ComponentBase, IDisposable
{
    [Inject] protected IActivityService ActivityService { get; set; } = default!;
    [Inject] protected TimeFormatter TimeFormatter { get; set; } = default!;

    [Parameter]
    public DateTime SelectedDate { get; set; }

    [Parameter]
    public List<ActivityRecord> Activities { get; set; } = new();

    private static readonly string BreakColor = "#1D9E75";
    private static readonly string[] TaskColors = { "#D85A30", "#E8913A", "#C75B9B", "#7B68EE", "#20B2AA", "#CD853F", "#6B8E23", "#DB7093" };

    public List<ChartSegment> Segments { get; set; } = new();
    public int TotalMinutes { get; set; }
    private bool _isDisposed;

    public string FormattedTotal
    {
        get
        {
            try
            {
                return TimeFormatter?.FormatTime(TotalMinutes) ?? TotalMinutes.ToString();
            }
            catch
            {
                return TotalMinutes.ToString();
            }
        }
    }

    protected override void OnInitialized()
    {
        ActivityService.OnActivityChanged += OnActivityChanged;
        CalculateSegments();
    }

    protected override void OnParametersSet()
    {
        CalculateSegments();
    }

    private void OnActivityChanged()
    {
        if (_isDisposed) return;
        InvokeAsync(() =>
        {
            CalculateSegments();
            StateHasChanged();
        });
    }

    private void CalculateSegments()
    {
        var distribution = ActivityService.GetTimeDistribution(SelectedDate);

        if (distribution == null || distribution.Count == 0)
        {
            Segments = new List<ChartSegment>();
            TotalMinutes = 0;
            return;
        }

        TotalMinutes = distribution.Values.Sum();
        var segments = new List<ChartSegment>();
        var taskIndex = 0;

        foreach (var kvp in distribution)
        {
            var label = kvp.Key;
            string color;

            if (label.Equals(Constants.Activity.BreaksLabel, StringComparison.OrdinalIgnoreCase))
            {
                color = BreakColor;
            }
            else
            {
                color = TaskColors[taskIndex % TaskColors.Length];
                taskIndex++;
            }

            var pct = TotalMinutes > 0 ? Math.Round((double)kvp.Value / TotalMinutes * 100) : 0;
            segments.Add(new ChartSegment(label, color, pct));
        }

        Segments = segments;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        ActivityService.OnActivityChanged -= OnActivityChanged;
    }

    public record ChartSegment(string Label, string Color, double Percentage);
}

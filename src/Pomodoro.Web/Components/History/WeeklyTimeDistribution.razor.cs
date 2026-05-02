using System.Linq;
using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Components.History;

public class WeeklyTimeDistributionBase : ComponentBase, IDisposable
{
    [Inject] protected IActivityService ActivityService { get; set; } = default!;
    [Inject] protected TimeFormatter TimeFormatter { get; set; } = default!;

    [Parameter]
    public Dictionary<DateTime, int> WeeklyFocusMinutes { get; set; } = new();

    [Parameter]
    public Dictionary<DateTime, int> WeeklyBreakMinutes { get; set; } = new();

    [Parameter]
    public DateTime WeekStart { get; set; }

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
        var weekEnd = WeekStart.AddDays(7);
        var weekActivities = ActivityService.GetAllActivities()
            .Where(a => a.CompletedAt.Date >= WeekStart.Date && a.CompletedAt.Date < weekEnd.Date)
            .ToList();

        Segments = new List<ChartSegment>();
        TotalMinutes = 0;

        if (weekActivities.Count == 0)
        {
            return;
        }

        var segments = new List<ChartSegment>();
        var taskIndex = 0;

        var pomodoroByTask = weekActivities
            .Where(a => a.Type == SessionType.Pomodoro)
            .GroupBy(a => a.TaskName ?? Constants.Activity.FocusTimeLabel)
            .Select(g => new { Label = g.Key, Minutes = g.Sum(a => a.DurationMinutes) })
            .OrderByDescending(x => x.Minutes);

        foreach (var task in pomodoroByTask)
        {
            segments.Add(new ChartSegment(task.Label, TaskColors[taskIndex % TaskColors.Length], 0));
            TotalMinutes += task.Minutes;
            taskIndex++;
        }

        var totalBreakMin = weekActivities
            .Where(a => a.Type == SessionType.ShortBreak || a.Type == SessionType.LongBreak)
            .Sum(a => a.DurationMinutes);

        if (totalBreakMin > 0)
        {
            segments.Add(new ChartSegment(Constants.Activity.BreaksLabel, BreakColor, 0));
            TotalMinutes += totalBreakMin;
        }

        segments = segments.Select(s => s with
        {
            Percentage = TotalMinutes > 0 ? Math.Round((double)GetSegmentMinutes(s.Label, weekActivities) / TotalMinutes * 100) : 0
        }).ToList();

        Segments = segments.OrderByDescending(s => s.Percentage).ToList();
    }

    private static int GetSegmentMinutes(string label, List<Models.ActivityRecord> activities)
    {
        if (label == Constants.Activity.BreaksLabel)
            return activities.Where(a => a.Type == SessionType.ShortBreak || a.Type == SessionType.LongBreak).Sum(a => a.DurationMinutes);
        return activities
            .Where(a => a.Type == SessionType.Pomodoro && (a.TaskName ?? Constants.Activity.FocusTimeLabel) == label)
            .Sum(a => a.DurationMinutes);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        ActivityService.OnActivityChanged -= OnActivityChanged;
    }

    public record ChartSegment(string Label, string Color, double Percentage);
}

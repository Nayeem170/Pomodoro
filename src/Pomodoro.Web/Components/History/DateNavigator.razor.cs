using Microsoft.AspNetCore.Components;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Components.History;

/// <summary>
/// Code-behind for DateNavigator component
/// Provides date navigation controls
/// </summary>
public class DateNavigatorBase : ComponentBase
{
    #region Lifecycle Methods
    
    protected override async Task OnInitializedAsync()
    {
        await UpdateFormattedDate();
        await UpdateIsTodayAsync();
        await base.OnInitializedAsync();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await UpdateFormattedDate();
        await UpdateIsTodayAsync();
        await base.OnParametersSetAsync();
    }
    
    #endregion
    
    #region Services
    
    [Inject]
    protected ILocalDateTimeService LocalDateTimeService { get; set; } = default!;
    
    #endregion
    
    #region Parameters
    
    [Parameter]
    public DateTime SelectedDate { get; set; }
    
    [Parameter]
    public EventCallback<DateTime> OnDateChanged { get; set; }
    
    #endregion

    #region Properties
    
    protected string FormattedDate { get; set; } = string.Empty;
    protected bool IsTodayAsyncValue { get; set; }
    
    protected async Task<bool> IsTodayAsync()
    {
        var localDate = await LocalDateTimeService.GetLocalDateAsync();
        return SelectedDate.Date == localDate;
    }
    
    protected bool IsToday => SelectedDate.Date == DateTime.Now.Date;
    
    #endregion

    #region Actions
    
    protected async Task GoToPrevious()
    {
        var newDate = SelectedDate.AddDays(Constants.TimeConversion.DayOffsetPrevious);
        await OnDateChanged.InvokeAsync(newDate);
    }
    
    protected async Task GoToNext()
    {
        var newDate = SelectedDate.AddDays(Constants.TimeConversion.DayOffsetNext);
        await OnDateChanged.InvokeAsync(newDate);
    }
    
    protected async Task GoToToday()
    {
        var localDate = await LocalDateTimeService.GetLocalDateAsync();
        await OnDateChanged.InvokeAsync(localDate);
    }
    
    #endregion

    #region Helper Methods
    
    protected async Task UpdateFormattedDate()
    {
        FormattedDate = await FormatDateAsync(SelectedDate);
    }
    
    protected async Task UpdateIsTodayAsync()
    {
        IsTodayAsyncValue = await IsTodayAsync();
    }
    
    protected async Task<string> FormatDateAsync(DateTime date)
    {
        var today = await LocalDateTimeService.GetLocalDateAsync();
        var yesterday = today.AddDays(Constants.TimeConversion.DayOffsetPrevious);
        
        if (date.Date == today)
            return Constants.TimeFormats.TodayText;
        if (date.Date == yesterday)
            return Constants.TimeFormats.YesterdayText;
        
        return date.ToString(Constants.TimeFormats.DateFormat);
    }
    
    protected string FormatDate(DateTime date)
    {
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(Constants.TimeConversion.DayOffsetPrevious);
        
        if (date.Date == today)
            return Constants.TimeFormats.TodayText;
        if (date.Date == yesterday)
            return Constants.TimeFormats.YesterdayText;
        
        return date.ToString(Constants.TimeFormats.DateFormat);
    }
    
    #endregion
}

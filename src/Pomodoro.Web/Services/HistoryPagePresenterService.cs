using Microsoft.Extensions.Logging;

namespace Pomodoro.Web.Services;

public class HistoryPagePresenterService
{
    private readonly ILogger<HistoryPagePresenterService> _logger;

    public HistoryPagePresenterService(ILogger<HistoryPagePresenterService> logger)
    {
        _logger = logger;
    }

    public virtual string FormatFocusTime(int minutes)
    {
        return minutes.ToString();
    }
}

public class HistoryPagePaginationState
{
    public int CurrentSkip { get; set; }
    public bool HasMoreActivities { get; set; }
    public bool ObserverInitialized { get; set; }
}

using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

public class ActivityRecordTestsCoverage
{
    [Fact]
    public void DisplayText_WithInvalidSessionType_ReturnsUnknownActivity()
    {
        var record = new ActivityRecord
        {
            Type = (SessionType)99
        };

        Assert.Equal(Constants.SessionTypes.UnknownActivity, record.DisplayText);
    }

    [Fact]
    public void Icon_WithInvalidSessionType_ReturnsTimerEmoji()
    {
        var record = new ActivityRecord
        {
            Type = (SessionType)99
        };

        Assert.Equal(Constants.SessionTypes.TimerEmoji, record.Icon);
    }
}

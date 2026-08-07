using FluentAssertions;
using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class ActivityServiceTests
{
    [Fact]
    public async Task HandleTimerCompletedAsync_WithWasCompletedFalse_CreatesPartialActivity()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var args = new TimerCompletedEventArgs(
            SessionType.Pomodoro,
            taskId,
            "Test Task",
            15,
            WasCompleted: false,
            DateTime.UtcNow
        );

        ActivityRecord? capturedActivity = null;
        MockActivityRepository
            .Setup(x => x.SaveAsync(It.IsAny<ActivityRecord>()))
            .Callback<ActivityRecord>(a => capturedActivity = a)
            .ReturnsAsync(true);

        var service = CreateService();
        var subscriber = (ITimerEventSubscriber)service;

        // Act
        await subscriber.HandleTimerCompletedAsync(args);

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.WasCompleted.Should().BeFalse();
        capturedActivity.DurationMinutes.Should().Be(15);
        capturedActivity.TaskId.Should().Be(taskId);
        capturedActivity.TaskName.Should().Be("Test Task");
        capturedActivity.Type.Should().Be(SessionType.Pomodoro);
    }

    [Fact]
    public async Task HandleTimerCompletedAsync_WithWasCompletedTrue_CreatesCompletedActivity()
    {
        // Arrange
        var args = new TimerCompletedEventArgs(
            SessionType.Pomodoro,
            Guid.NewGuid(),
            "Test Task",
            25,
            WasCompleted: true,
            DateTime.UtcNow
        );

        ActivityRecord? capturedActivity = null;
        MockActivityRepository
            .Setup(x => x.SaveAsync(It.IsAny<ActivityRecord>()))
            .Callback<ActivityRecord>(a => capturedActivity = a)
            .ReturnsAsync(true);

        var service = CreateService();
        var subscriber = (ITimerEventSubscriber)service;

        // Act
        await subscriber.HandleTimerCompletedAsync(args);

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.WasCompleted.Should().BeTrue();
        capturedActivity.DurationMinutes.Should().Be(25);
    }
}

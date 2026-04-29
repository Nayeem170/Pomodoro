using Moq;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService session type transitions.
/// </summary>
[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    public class TransitionsTests : TimerServiceTests
    {
        [Fact]
        public async Task SwitchSessionTypeAsync_FromPomodoroToShortBreak_SwitchesCorrectly()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartPomodoroAsync();

            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Assert
            Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.Equal(service.Settings.GetDurationSeconds(SessionType.ShortBreak), service.RemainingTime.TotalSeconds);
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_FromPomodoroToLongBreak_SwitchesCorrectly()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartPomodoroAsync();

            // Act
            await service.SwitchSessionTypeAsync(SessionType.LongBreak);

            // Assert
            Assert.Equal(SessionType.LongBreak, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.Equal(service.Settings.GetDurationSeconds(SessionType.LongBreak), service.RemainingTime.TotalSeconds);
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_FromShortBreakToPomodoro_SwitchesCorrectly()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartShortBreakAsync();

            // Act
            await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

            // Assert
            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.Equal(service.Settings.GetDurationSeconds(SessionType.Pomodoro), service.RemainingTime.TotalSeconds);
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_FromLongBreakToPomodoro_SwitchesCorrectly()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartLongBreakAsync();

            // Act
            await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

            // Assert
            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.Equal(service.Settings.GetDurationSeconds(SessionType.Pomodoro), service.RemainingTime.TotalSeconds);
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_FromShortBreakToLongBreak_SwitchesCorrectly()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartShortBreakAsync();

            // Act
            await service.SwitchSessionTypeAsync(SessionType.LongBreak);

            // Assert
            Assert.Equal(SessionType.LongBreak, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.Equal(service.Settings.GetDurationSeconds(SessionType.LongBreak), service.RemainingTime.TotalSeconds);
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_FromLongBreakToShortBreak_SwitchesCorrectly()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartLongBreakAsync();

            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Assert
            Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.Equal(service.Settings.GetDurationSeconds(SessionType.ShortBreak), service.RemainingTime.TotalSeconds);
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_ToSameSessionType_PreservesState()
        {
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartPomodoroAsync();
            var durationBefore = service.RemainingTime.TotalSeconds;

            await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.True(service.IsPaused);
            Assert.Equal(durationBefore, service.RemainingTime.TotalSeconds);
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_FiresOnStateChangedEvent()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartPomodoroAsync();
            var eventFired = false;
            service.OnTimerStateChanged += () => eventFired = true;

            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task StartPomodoroAsync_AfterBreak_StartsNewPomodoroSession()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartShortBreakAsync();
            await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

            // Act
            await service.StartPomodoroAsync();

            // Assert
            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
            Assert.True(service.IsRunning);
        }

        [Fact]
        public async Task StartShortBreakAsync_AfterPomodoro_StartsNewBreakSession()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartPomodoroAsync();
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Act
            await service.StartShortBreakAsync();

            // Assert
            Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
            Assert.True(service.IsRunning);
        }

        [Fact]
        public async Task FullPomodoroCycle_TransitionsCorrectly()
        {
            var service = CreateService();
            await service.InitializeAsync();

            await service.StartPomodoroAsync();
            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
            Assert.True(service.IsRunning);

            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);
            Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
            Assert.False(service.IsRunning);

            await service.StartShortBreakAsync();
            Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
            Assert.True(service.IsRunning);

            await service.SwitchSessionTypeAsync(SessionType.Pomodoro);
            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
            Assert.False(service.IsRunning);
            Assert.True(service.IsPaused);
        }
    }
}

[Trait("Category", "Service")]
public class SwitchSessionPreservationTests : TimerServiceTests
{
    [Fact]
    public async Task SwitchAwayAndBack_PreservesRemainingTime()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();

        AppState.CurrentSession!.RemainingSeconds = 1200;

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);
        await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

        Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
        Assert.Equal(1200, service.RemainingSeconds);
        Assert.True(service.IsPaused);
    }

    [Fact]
    public async Task SwitchAwayAndBack_PreservesTaskAssociation()
    {
        var service = CreateService();
        await service.InitializeAsync();
        var taskId = Guid.NewGuid();
        await service.StartPomodoroAsync(taskId);

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);
        await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

        Assert.Equal(taskId, service.CurrentSession!.TaskId);
    }

    [Fact]
    public async Task SwitchToNewType_CreatesFreshSession()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

        Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
        Assert.Equal(service.Settings.GetDurationSeconds(SessionType.ShortBreak), service.RemainingSeconds);
        Assert.False(service.IsPaused);
    }

    [Fact]
    public async Task ResetAsync_OnlyClearsCurrentSessionPausedState()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();
        AppState.CurrentSession!.RemainingSeconds = 1200;

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);
        await service.ResetAsync();
        await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

        Assert.Equal(1200, service.RemainingSeconds);
        Assert.True(service.IsPaused);
    }

    [Fact]
    public async Task SwitchWhileNotStarted_CreatesFreshSessionForTarget()
    {
        var service = CreateService();
        await service.InitializeAsync();

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

        Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
        Assert.Equal(service.Settings.GetDurationSeconds(SessionType.ShortBreak), service.RemainingSeconds);
        Assert.False(service.IsPaused);
    }
}


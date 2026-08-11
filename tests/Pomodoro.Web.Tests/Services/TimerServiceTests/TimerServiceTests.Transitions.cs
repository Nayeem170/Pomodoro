using FluentAssertions;
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
        public async Task SwitchSessionTypeAsync_ToSameSessionType_IsNoOp()
        {
            var service = CreateService();
            await service.InitializeAsync();
            await service.StartPomodoroAsync();
            var durationBefore = service.RemainingTime.TotalSeconds;

            await service.SwitchSessionTypeAsync(SessionType.Pomodoro);

            Assert.Equal(SessionType.Pomodoro, service.CurrentSessionType);
            Assert.True(service.IsRunning);
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

            // Switching back restores the preserved Pomodoro as paused (it is
            // not abandoned by starting the break). See reset-session-isolation
            // and session-switch-preservation e2e specs.
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
    public async Task TabSwitch_PausesCurrentTimer_DoesNotRecordPartial()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();
        AppState.CurrentSession!.RemainingSeconds = 900; // 10 min elapsed
        TimerCompletedEventArgs? captured = null;
        service.OnSessionInterrupted += args => { captured = args; return Task.CompletedTask; };

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

        Assert.Null(captured); // tab switch must not log anything
        Assert.False(service.IsRunning);
    }

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
        Assert.True(service.IsPaused); // preserved as paused, ready to resume
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
    public async Task SwitchToNewType_TargetIsFresh()
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
    public async Task SwitchWhileNotStarted_CreatesFreshSessionForTarget()
    {
        var service = CreateService();
        await service.InitializeAsync();

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

        Assert.Equal(SessionType.ShortBreak, service.CurrentSessionType);
        Assert.Equal(service.Settings.GetDurationSeconds(SessionType.ShortBreak), service.RemainingSeconds);
        Assert.False(service.IsPaused);
    }

    [Fact]
    public async Task StartDifferentTimer_PreservesPausedPomodoro_ForResume()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();
        AppState.CurrentSession!.RemainingSeconds = 900; // 10 min elapsed
        TimerCompletedEventArgs? captured = null;
        service.OnSessionInterrupted += args => { captured = args; return Task.CompletedTask; };

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak); // Pomodoro paused+preserved
        await service.StartShortBreakAsync(); // must NOT abandon the paused Pomodoro

        Assert.Null(captured); // preserved, not recorded as a partial

        // Switching back must restore the paused Pomodoro with its remaining time.
        await service.SwitchSessionTypeAsync(SessionType.Pomodoro);
        Assert.Equal(900, service.RemainingSeconds);
        Assert.True(service.IsPaused);
    }

    [Fact]
    public async Task StartDifferentTimer_PreservesPausedBreak_ForResume()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartShortBreakAsync();
        AppState.CurrentSession!.RemainingSeconds = 60; // some elapsed break time
        TimerCompletedEventArgs? captured = null;
        service.OnSessionInterrupted += args => { captured = args; return Task.CompletedTask; };

        await service.SwitchSessionTypeAsync(SessionType.Pomodoro); // ShortBreak paused+preserved
        await service.StartPomodoroAsync(); // breaks never record partials, and are preserved

        Assert.Null(captured);

        // Switching back restores the paused break with its remaining time.
        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);
        Assert.Equal(60, service.RemainingSeconds);
        Assert.True(service.IsPaused);
    }

    [Fact]
    public async Task StartDifferentTimer_AbandonsRunningPomodoro_AsPartial()
    {
        // Ctrl+S while a Pomodoro is running (no tab switch first) abandons it.
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();
        AppState.CurrentSession!.RemainingSeconds = 900;
        TimerCompletedEventArgs? captured = null;
        service.OnSessionInterrupted += args => { captured = args; return Task.CompletedTask; };

        await service.StartShortBreakAsync();

        Assert.NotNull(captured);
        captured!.SessionType.Should().Be(SessionType.Pomodoro);
        captured.DurationMinutes.Should().Be(10);
    }

    [Fact]
    public async Task StartSameType_AlreadyRunning_IsNoOp()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();
        var remainingBefore = service.RemainingSeconds;

        await service.StartPomodoroAsync(); // second start while running

        Assert.True(service.IsRunning);
        Assert.Equal(remainingBefore, service.RemainingSeconds); // not restarted
    }

    [Fact]
    public async Task ResetClearsSameTypePausedEntry()
    {
        var service = CreateService();
        await service.InitializeAsync();
        await service.StartPomodoroAsync();
        AppState.CurrentSession!.RemainingSeconds = 1200;

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak); // Pomodoro stored at 1200
        await service.SwitchSessionTypeAsync(SessionType.Pomodoro); // restored, paused
        Assert.True(service.IsPaused);

        await service.ResetAsync(); // resets current Pomodoro, clears stored Pomodoro entry

        Assert.Equal(service.Settings.GetDurationSeconds(SessionType.Pomodoro), service.RemainingSeconds);
        Assert.False(service.IsPaused);

        await service.SwitchSessionTypeAsync(SessionType.ShortBreak);
        await service.SwitchSessionTypeAsync(SessionType.Pomodoro); // no stored entry -> fresh

        Assert.Equal(service.Settings.GetDurationSeconds(SessionType.Pomodoro), service.RemainingSeconds);
        Assert.False(service.IsPaused);
    }
}


using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class TimerServiceTests
{
    public class PartialSessionTests : TimerServiceTests
    {
        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenRecordDisabled_ReturnsFalse()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = false;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenPausedWithElapsed_RecordsPartial()
        {
            // Arrange - session was started then paused (IsRunning=false, WasStarted=true)
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeTrue(
                "a paused session with elapsed time must be recorded as a partial session");
            capturedArgs.Should().NotBeNull();
            capturedArgs!.WasCompleted.Should().BeFalse();
            capturedArgs.DurationMinutes.Should().Be(10);
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenSessionNotStarted_ReturnsFalse()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: false, remainingSeconds: 900);
            var service = CreateService();

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenNoSession_ReturnsFalse()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            ClearCurrentSession();
            var service = CreateService();

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenElapsedBelowThreshold_ReturnsFalse()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 1500 - 59);
            var service = CreateService();

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_AtExactly60Seconds_ReturnsTrue()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 1500 - 60);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeTrue();
            capturedArgs.Should().NotBeNull();
            capturedArgs!.DurationMinutes.Should().Be(1);
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenConditionsMet_FiresEventAndReturnsTrue()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 1500 - 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeTrue();
            capturedArgs.Should().NotBeNull();
            capturedArgs!.WasCompleted.Should().BeFalse();
            capturedArgs.DurationMinutes.Should().Be(10);
            capturedArgs.SessionType.Should().Be(SessionType.Pomodoro);
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_ComputesCorrectElapsedMinutes()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 1500 - 90);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeTrue();
            capturedArgs!.DurationMinutes.Should().Be(2);
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_ForBreakSession_DoesNotRecord()
        {
            // Arrange - only Pomodoros are logged as partials. Breaks are rest
            // time, so abandoning a break never produces an activity record.
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 1500 - 180, sessionType: SessionType.ShortBreak);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeFalse();
            capturedArgs.Should().BeNull();
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenTaskExists_ResolvesTaskName()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            var taskId = Guid.NewGuid();
            AppState.Tasks = new List<TaskItem> { new() { Id = taskId, Name = "My Task" } };
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 900);
            AppState.CurrentSession!.TaskId = taskId;
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeTrue();
            capturedArgs!.TaskName.Should().Be("My Task");
            capturedArgs.TaskId.Should().Be(taskId);
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenNoTask_TaskNameIsNull()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeTrue();
            capturedArgs!.TaskName.Should().BeNull();
            capturedArgs.TaskId.Should().BeNull();
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_DoesNotModifySessionState()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();

            // Act
            await service.TryRecordPartialSessionAsync();

            // Assert
            AppState.CurrentSession!.IsRunning.Should().BeTrue();
            AppState.CurrentSession.WasStarted.Should().BeTrue();
            AppState.CurrentSession.RemainingSeconds.Should().Be(900);
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_DoesNotFireOnTimerCompleted()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();
            var timerCompletedFired = false;
            service.OnTimerCompleted += _ => { timerCompletedFired = true; return Task.CompletedTask; };
            var sessionInterruptedFired = false;
            service.OnSessionInterrupted += _ => { sessionInterruptedFired = true; return Task.CompletedTask; };

            // Act
            await service.TryRecordPartialSessionAsync();

            // Assert
            sessionInterruptedFired.Should().BeTrue();
            timerCompletedFired.Should().BeFalse();
        }

        [Fact]
        public async Task ResetAsync_WhenPartialRecordingOn_RecordsPartialBeforeReset()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.ResetAsync();

            // Assert
            capturedArgs.Should().NotBeNull();
            capturedArgs!.WasCompleted.Should().BeFalse();
            capturedArgs.DurationMinutes.Should().Be(15);
            AppState.CurrentSession!.IsRunning.Should().BeFalse();
            AppState.CurrentSession.WasStarted.Should().BeFalse();
        }

        [Fact]
        public async Task ResetAsync_WhenPartialRecordingOff_DoesNotRecord()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = false;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.ResetAsync();

            // Assert
            capturedArgs.Should().BeNull();
        }

        [Fact]
        public async Task ResetAsync_WhenSessionNotStarted_DoesNotRecord()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: false, wasStarted: false, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.ResetAsync();

            // Assert
            capturedArgs.Should().BeNull();
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_WhenPartialRecordingOn_DoesNotRecord()
        {
            // Tab switching only pauses and preserves the current timer; it must
            // not log a partial. The partial is recorded only when a different
            // timer is later started, which abandons the preserved one.
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Assert
            capturedArgs.Should().BeNull();
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_WhenPartialRecordingOff_DoesNotRecord()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = false;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Assert
            capturedArgs.Should().BeNull();
        }

        [Fact]
        public async Task ResetAsync_WhenPaused_RecordsPartialBeforeReset()
        {
            // Arrange - user started timer, paused, then reset
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.ResetAsync();

            // Assert
            capturedArgs.Should().NotBeNull(
                "a paused-then-reset session must still log the partial elapsed time");
            capturedArgs!.WasCompleted.Should().BeFalse();
            capturedArgs.DurationMinutes.Should().Be(15);
            AppState.CurrentSession!.IsRunning.Should().BeFalse();
            AppState.CurrentSession.WasStarted.Should().BeFalse();
        }

        [Fact]
        public async Task SwitchSessionTypeAsync_WhenPaused_DoesNotRecord()
        {
            // A paused-then-switched session is preserved, not logged. It will
            // only log if a different timer is started afterwards (abandoning it).
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Assert
            capturedArgs.Should().BeNull();
        }

        [Fact]
        public async Task StartShortBreakAsync_WhilePomodoroRunning_RecordsPomodoroPartial()
        {
            // Arrange - a Pomodoro is running (keyboard shortcut path bypasses
            // SwitchSessionTypeAsync, going straight to StartShortBreakAsync)
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.StartShortBreakAsync();

            // Assert
            capturedArgs.Should().NotBeNull(
                "starting a break while a Pomodoro is running must record the Pomodoro as a partial session");
            capturedArgs!.WasCompleted.Should().BeFalse();
            capturedArgs.DurationMinutes.Should().Be(10);
            capturedArgs.SessionType.Should().Be(SessionType.Pomodoro);
        }

        [Fact]
        public async Task StartLongBreakAsync_WhilePomodoroRunning_RecordsPomodoroPartial()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.StartLongBreakAsync();

            // Assert
            capturedArgs.Should().NotBeNull();
            capturedArgs!.DurationMinutes.Should().Be(15);
            capturedArgs.SessionType.Should().Be(SessionType.Pomodoro);
        }

        [Fact]
        public async Task StartSessionAsync_WhenNoSessionInProgress_DoesNotRecord()
        {
            // Arrange - no prior session (fresh start) must not fire a phantom partial
            AppState.Settings.RecordPartialSessions = true;
            ClearCurrentSession();
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.StartShortBreakAsync();

            // Assert
            capturedArgs.Should().BeNull();
        }

        [Fact]
        public async Task TryRecordPartialSessionAsync_WhenSubscriberThrows_LogsAndDoesNotPropagate()
        {
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();
            service.OnSessionInterrupted += _ => throw new InvalidOperationException("subscriber failed");

            var result = await service.TryRecordPartialSessionAsync();

            result.Should().BeTrue("the partial is still recorded when a subscriber throws");
            MockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => v != null), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}

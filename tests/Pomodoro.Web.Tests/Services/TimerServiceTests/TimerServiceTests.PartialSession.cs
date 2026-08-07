using FluentAssertions;
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
        public async Task TryRecordPartialSessionAsync_WhenSessionNotRunning_ReturnsFalse()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: false, wasStarted: true, remainingSeconds: 900);
            var service = CreateService();

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeFalse();
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
        public async Task TryRecordPartialSessionAsync_ForBreakSession_RecordsPartial()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 1500 - 180, sessionType: SessionType.ShortBreak);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            var result = await service.TryRecordPartialSessionAsync();

            // Assert
            result.Should().BeTrue();
            capturedArgs!.SessionType.Should().Be(SessionType.ShortBreak);
            capturedArgs.WasCompleted.Should().BeFalse();
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
        public async Task SwitchSessionTypeAsync_WhenPartialRecordingOn_RecordsPartialBeforeSwitch()
        {
            // Arrange
            AppState.Settings.RecordPartialSessions = true;
            SetupCurrentSession(isRunning: true, wasStarted: true, remainingSeconds: 600);
            var service = CreateService();
            TimerCompletedEventArgs? capturedArgs = null;
            service.OnSessionInterrupted += args => { capturedArgs = args; return Task.CompletedTask; };

            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);

            // Assert
            capturedArgs.Should().NotBeNull();
            capturedArgs!.WasCompleted.Should().BeFalse();
            capturedArgs.DurationMinutes.Should().Be(15);
            capturedArgs.SessionType.Should().Be(SessionType.Pomodoro);
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
    }
}

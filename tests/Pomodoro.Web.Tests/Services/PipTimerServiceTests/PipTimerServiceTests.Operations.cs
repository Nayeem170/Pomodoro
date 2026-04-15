using Xunit;
using Pomodoro.Web.Models;
using Moq;
using Microsoft.Extensions.Logging;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for PipTimerService JSInvokable operations.
/// </summary>
[Trait("Category", "Service")]
public partial class PipTimerServiceTests
{
    /// <summary>
    /// Tests for OnPipToggleTimer operation
    /// </summary>
    [Trait("Category", "Service")]
    public class OperationsTests : PipTimerServiceTests
    {
        [Fact]
        public async Task OnPipToggleTimer_WhenRunning_PausesTimer()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true);
            var service = CreateService();

            // Act
            await service.OnPipToggleTimer();

            // Assert
            MockTimerService.Verify(t => t.PauseAsync(), Times.Once);
        }

        [Fact]
        public async Task OnPipToggleTimer_WhenPaused_ResumesTimer()
        {
            // Arrange
            SetupTimerState(isRunning: false, isStarted: true); // IsPaused = true
            var service = CreateService();

            // Act
            await service.OnPipToggleTimer();

            // Assert
            MockTimerService.Verify(t => t.ResumeAsync(), Times.Once);
        }

        [Fact]
        public async Task OnPipToggleTimer_WhenNotStarted_PomodoroWithTask_StartsPomodoro()
        {
            // Arrange
            SetupTimerState(isRunning: false, isStarted: false, sessionType: SessionType.Pomodoro);
            var taskId = Guid.NewGuid();
            MockTaskService.SetupGet(t => t.CurrentTaskId).Returns(taskId);
            var service = CreateService();

            // Act
            await service.OnPipToggleTimer();

            // Assert
            MockTimerService.Verify(t => t.StartPomodoroAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task OnPipToggleTimer_WhenNotStarted_PomodoroWithoutTask_DoesNotStart()
        {
            // Arrange
            SetupTimerState(isRunning: false, isStarted: false, sessionType: SessionType.Pomodoro);
            MockTaskService.SetupGet(t => t.CurrentTaskId).Returns((Guid?)null);
            var service = CreateService();

            // Act
            await service.OnPipToggleTimer();

            // Assert
            MockTimerService.Verify(t => t.StartPomodoroAsync(It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task OnPipToggleTimer_WhenNotStarted_ShortBreak_StartsShortBreak()
        {
            // Arrange
            SetupTimerState(isRunning: false, isStarted: false, sessionType: SessionType.ShortBreak);
            var service = CreateService();

            // Act
            await service.OnPipToggleTimer();

            // Assert
            MockTimerService.Verify(t => t.StartShortBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task OnPipToggleTimer_WhenNotStarted_LongBreak_StartsLongBreak()
        {
            // Arrange
            SetupTimerState(isRunning: false, isStarted: false, sessionType: SessionType.LongBreak);
            var service = CreateService();

            // Act
            await service.OnPipToggleTimer();

            // Assert
            MockTimerService.Verify(t => t.StartLongBreakAsync(), Times.Once);
        }

        [Fact]
        public async Task OnPipResetTimer_CallsResetAsync()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.OnPipResetTimer();

            // Assert
            MockTimerService.Verify(t => t.ResetAsync(), Times.Once);
        }

        [Fact]
        public async Task OnPipResetTimer_WhenResetThrowsException_LogsError()
        {
            // Arrange
            MockTimerService
                .Setup(t => t.ResetAsync())
                .ThrowsAsync(new InvalidOperationException("Reset failed"));
            var service = CreateService();

            // Act
            await service.OnPipResetTimer();

            // Assert
            MockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task OnPipSwitchSession_WithPomodoro_SwitchesToPomodoro()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.OnPipSwitchSession((int)SessionType.Pomodoro);

            // Assert
            MockTimerService.Verify(t => t.SwitchSessionTypeAsync(SessionType.Pomodoro), Times.Once);
        }

        [Fact]
        public async Task OnPipSwitchSession_WithShortBreak_SwitchesToShortBreak()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.OnPipSwitchSession((int)SessionType.ShortBreak);

            // Assert
            MockTimerService.Verify(t => t.SwitchSessionTypeAsync(SessionType.ShortBreak), Times.Once);
        }

        [Fact]
        public async Task OnPipSwitchSession_WithLongBreak_SwitchesToLongBreak()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.OnPipSwitchSession((int)SessionType.LongBreak);

            // Assert
            MockTimerService.Verify(t => t.SwitchSessionTypeAsync(SessionType.LongBreak), Times.Once);
        }
    }
}


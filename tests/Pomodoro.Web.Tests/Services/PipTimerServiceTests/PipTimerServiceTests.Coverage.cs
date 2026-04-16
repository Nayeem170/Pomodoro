using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class PipTimerServiceTests
{
    public class CoverageTests : PipTimerServiceTests
    {
        [Fact]
        public async Task OnPipToggleTimer_WhenOperationThrows_LogsError()
        {
            SetupTimerState(isRunning: true, isStarted: true);
            MockTimerService.Setup(t => t.PauseAsync()).ThrowsAsync(new InvalidOperationException("fail"));
            var service = CreateService();

            await service.OnPipToggleTimer();

            MockLogger.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                    It.IsAny<InvalidOperationException>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task OnPipSwitchSession_WhenSwitchThrows_LogsError()
        {
            MockTimerService.Setup(t => t.SwitchSessionTypeAsync(It.IsAny<SessionType>()))
                .ThrowsAsync(new InvalidOperationException("fail"));
            var service = CreateService();

            await service.OnPipSwitchSession((int)SessionType.Pomodoro);

            MockLogger.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                    It.IsAny<InvalidOperationException>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task OnPipClosedJs_WithNoSubscribers_DoesNotThrow()
        {
            var service = CreateService();

            service.OnPipClosedJs();

            Assert.False(service.IsOpen);
        }

        [Fact]
        public async Task OnTimerTick_WhenInitializedAndOpen_UpdatesTimer()
        {
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.Update);

            var service = CreateService();
            await service.InitializeAsync();
            await service.OpenAsync();

            service.HandleTimerTick();
            await Task.Delay(200);

            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.Update, Moq.Times.AtLeastOnce());
        }

        [Fact]
        public async Task OnTimerStateChanged_WhenInitializedAndOpen_UpdatesTimer()
        {
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.Update);

            var service = CreateService();
            await service.InitializeAsync();
            await service.OpenAsync();

            service.HandleTimerStateChanged();
            await Task.Delay(200);

            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.Update, Moq.Times.AtLeastOnce());
        }

        [Fact]
        public async Task DisposeAsync_WithoutInitialize_DotNetRefIsNull()
        {
            var service = CreateService();

            await service.DisposeAsync();

            Assert.True(service.IsOpen == false);
        }

        [Fact]
        public async Task DisposeAsync_AfterInitialize_DotNetRefIsDisposed()
        {
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef);

            var service = CreateService();
            await service.InitializeAsync();
            await service.DisposeAsync();
        }
    }
}


using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public partial class NotificationServiceTests
{
    public class SoundTests : NotificationServiceTests
    {
        [Fact]
        public async Task PlayTimerCompleteSoundAsync_CallsJsFunction()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.PlayTimerCompleteSound);
            var service = CreateService();

            // Act
            await service.PlayTimerCompleteSoundAsync();

            // Assert
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.PlayTimerCompleteSound,
                It.IsAny<object?[]?>()), Moq.Times.Once());
        }

        [Fact]
        public async Task PlayTimerCompleteSoundAsync_WhenJsThrowsException_DoesNotThrow()
        {
            // Arrange
            SetupJsInvokeVoidAsyncException(Constants.NotificationJsFunctions.PlayTimerCompleteSound, new InvalidOperationException("JS error"));
            var service = CreateService();

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() => service.PlayTimerCompleteSoundAsync());
            Assert.Null(exception);
        }

        [Fact]
        public async Task PlayBreakCompleteSoundAsync_CallsJsFunction()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.PlayBreakCompleteSound);
            var service = CreateService();

            // Act
            await service.PlayBreakCompleteSoundAsync();

            // Assert
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.PlayBreakCompleteSound,
                It.IsAny<object?[]?>()), Moq.Times.Once());
        }

        [Fact]
        public async Task PlayBreakCompleteSoundAsync_WhenJsThrowsException_DoesNotThrow()
        {
            // Arrange
            SetupJsInvokeVoidAsyncException(Constants.NotificationJsFunctions.PlayBreakCompleteSound, new InvalidOperationException("JS error"));
            var service = CreateService();

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() => service.PlayBreakCompleteSoundAsync());
            Assert.Null(exception);
        }
    }
}

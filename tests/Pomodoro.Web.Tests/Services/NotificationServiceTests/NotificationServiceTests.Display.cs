using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class NotificationServiceTests
{
    public class DisplayTests : NotificationServiceTests
    {
        [Fact]
        public async Task ShowNotificationAsync_WhenPermitted_CallsJsFunction()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.ShowNotification);
            var service = CreateService();
            await service.RequestPermissionAsync();

            // Act
            await service.ShowNotificationAsync("Test Title", "Test Body", SessionType.Pomodoro);

            // Assert
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.ShowNotification,
                It.IsAny<object?[]?>()), Moq.Times.Once());
        }

        [Fact]
        public async Task ShowNotificationAsync_WhenNotPermitted_DoesNotCallJsFunction()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, "denied");
            var service = CreateService();
            await service.RequestPermissionAsync();

            // Act
            await service.ShowNotificationAsync("Test Title", "Test Body", SessionType.Pomodoro);

            // Assert
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.ShowNotification,
                It.IsAny<object?[]?>()), Moq.Times.Never());
        }

        [Fact]
        public async Task ShowNotificationAsync_WithShortBreakSessionType_PassesCorrectSessionType()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.ShowNotification);
            var service = CreateService();
            await service.RequestPermissionAsync();

            // Act
            await service.ShowNotificationAsync("Break Complete", "Time to focus!", SessionType.ShortBreak);

            // Assert - SessionType.ShortBreak = 1
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.ShowNotification,
                It.Is<object?[]?>(args => args != null && args.Length >= 4 && (int)args[3]! == 1)), Moq.Times.Once());
        }

        [Fact]
        public async Task ShowNotificationAsync_WithLongBreakSessionType_PassesCorrectSessionType()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.ShowNotification);
            var service = CreateService();
            await service.RequestPermissionAsync();

            // Act
            await service.ShowNotificationAsync("Long Break Complete", "Time to focus!", SessionType.LongBreak);

            // Assert - SessionType.LongBreak = 2
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.ShowNotification,
                It.Is<object?[]?>(args => args != null && args.Length >= 4 && (int)args[3]! == 2)), Moq.Times.Once());
        }

        [Fact]
        public async Task ShowNotificationAsync_WithPomodoroSessionType_PassesCorrectSessionType()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.ShowNotification);
            var service = CreateService();
            await service.RequestPermissionAsync();

            // Act
            await service.ShowNotificationAsync("Pomodoro Complete", "Time for a break!", SessionType.Pomodoro);

            // Assert - SessionType.Pomodoro = 0
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.ShowNotification,
                It.Is<object?[]?>(args => args != null && args.Length >= 4 && (int)args[3]! == 0)), Moq.Times.Once());
        }

        [Fact]
        public async Task ShowNotificationAsync_WithIcon_PassesIconToJs()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.ShowNotification);
            var service = CreateService();
            await service.RequestPermissionAsync();

            // Act
            await service.ShowNotificationAsync("Test", "Body", SessionType.Pomodoro, "/icons/timer.png");

            // Assert - icon is 3rd arg (index 2)
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.ShowNotification,
                It.Is<object?[]?>(args => ArgumentMatches(args, 2, "/icons/timer.png"))), Moq.Times.Once());
        }

        [Fact]
        public async Task ShowNotificationAsync_WhenJsThrowsException_DoesNotThrow()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            SetupJsInvokeVoidAsyncException(Constants.NotificationJsFunctions.ShowNotification, new InvalidOperationException("JS error"));
            var service = CreateService();
            await service.RequestPermissionAsync();

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() =>
                service.ShowNotificationAsync("Test", "Body", SessionType.Pomodoro));
            Assert.Null(exception);
        }
    }
}


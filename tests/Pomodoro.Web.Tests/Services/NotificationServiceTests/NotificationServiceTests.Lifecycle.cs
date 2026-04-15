using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public partial class NotificationServiceTests
{
    [Trait("Category", "Service")]
    public class LifecycleTests : NotificationServiceTests
    {
        [Fact]
        public async Task InitializeAsync_RegistersDotNetRefAndRequestsPermission()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.RegisterDotNetRef);
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            var service = CreateService();

            // Act
            await service.InitializeAsync();

            // Assert
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.RegisterDotNetRef,
                It.IsAny<object?[]?>()), Moq.Times.Once());
            MockJsRuntime.Verify(js => js.InvokeAsync<string>(
                Constants.NotificationJsFunctions.RequestPermission,
                It.IsAny<object?[]?>()), Moq.Times.Once());
        }

        [Fact]
        public async Task InitializeAsync_WhenCalledTwice_OnlyInitializesOnce()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.RegisterDotNetRef);
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            var service = CreateService();

            // Act
            await service.InitializeAsync();
            await service.InitializeAsync();

            // Assert - Should only be called once
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.RegisterDotNetRef,
                It.IsAny<object?[]?>()), Moq.Times.Once());
        }

        [Fact]
        public async Task InitializeAsync_WhenJsThrowsException_DoesNotThrow()
        {
            // Arrange
            SetupJsInvokeVoidAsyncException(Constants.NotificationJsFunctions.RegisterDotNetRef, new InvalidOperationException("JS error"));
            var service = CreateService();

            // Act & Assert - Should not throw
            var exception = await Record.ExceptionAsync(() => service.InitializeAsync());
            Assert.Null(exception);
        }

        [Fact]
        public void OnNotificationActionClick_WhenSubscribed_RaisesEvent()
        {
            // Arrange
            var service = CreateService();
            string? receivedAction = null;
            service.OnNotificationAction += (action) => receivedAction = action;

            // Act
            service.OnNotificationActionClick("start");

            // Assert
            Assert.Equal("start", receivedAction);
        }

        [Fact]
        public void OnNotificationActionClick_WithNoSubscriber_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert - Should not throw
            var exception = Record.Exception(() => service.OnNotificationActionClick("start"));
            Assert.Null(exception);
        }

        [Fact]
        public async Task DisposeAsync_UnregistersDotNetRef()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.RegisterDotNetRef);
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.UnregisterDotNetRef);
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            var service = CreateService();
            await service.InitializeAsync();

            // Act
            await service.DisposeAsync();

            // Assert
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.UnregisterDotNetRef,
                It.IsAny<object?[]?>()), Moq.Times.Once());
        }

        [Fact]
        public async Task DisposeAsync_WhenJsThrowsException_DoesNotThrow()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.RegisterDotNetRef);
            SetupJsInvokeVoidAsyncException(Constants.NotificationJsFunctions.UnregisterDotNetRef, new InvalidOperationException("JS error"));
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            var service = CreateService();
            await service.InitializeAsync();

            // Act & Assert - Should not throw
            await service.DisposeAsync();
            // If we reach here without exception, the test passes
        }

        [Fact]
        public async Task DisposeAsync_WithoutInitialization_DoesNotThrow()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.UnregisterDotNetRef);
            var service = CreateService();

            // Act & Assert - Should not throw when _dotNetRef is null
            await service.DisposeAsync();
        }

        [Fact]
        public async Task InitializeCoreAsync_WhenDotNetRefAlreadySet_DisposesOldRef()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.NotificationJsFunctions.RegisterDotNetRef);
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            var service = CreateService();
            await service.InitializeAsync();

            // Act - Call InitializeCoreAsync again via reflection (bypasses _initializationTask guard)
            var method = typeof(NotificationService).GetMethod("InitializeCoreAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            await (Task)method.Invoke(service, null)!;

            // Assert - RegisterDotNetRef should be called twice (once for each init)
            MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(
                Constants.NotificationJsFunctions.RegisterDotNetRef,
                It.IsAny<object?[]?>()), Moq.Times.Exactly(2));
        }
    }
}


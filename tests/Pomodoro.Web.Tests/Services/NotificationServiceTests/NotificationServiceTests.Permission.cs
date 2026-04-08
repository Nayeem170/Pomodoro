using Pomodoro.Web;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public partial class NotificationServiceTests
{
    public class PermissionTests : NotificationServiceTests
    {
        [Fact]
        public async Task RequestPermissionAsync_WhenGranted_ReturnsTrue()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            var service = CreateService();
            
            // Act
            var result = await service.RequestPermissionAsync();
            
            // Assert
            Assert.True(result);
            Assert.True(service.IsNotificationPermitted);
        }

        [Fact]
        public async Task RequestPermissionAsync_WhenDenied_ReturnsFalse()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, "denied");
            var service = CreateService();
            
            // Act
            var result = await service.RequestPermissionAsync();
            
            // Assert
            Assert.False(result);
            Assert.False(service.IsNotificationPermitted);
        }

        [Fact]
        public async Task RequestPermissionAsync_WhenDefault_ReturnsFalse()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, "default");
            var service = CreateService();
            
            // Act
            var result = await service.RequestPermissionAsync();
            
            // Assert
            Assert.False(result);
            Assert.False(service.IsNotificationPermitted);
        }

        [Fact]
        public async Task RequestPermissionAsync_WhenJsThrowsException_ReturnsFalse()
        {
            // Arrange
            SetupJsInvokeAsyncException<string>(Constants.NotificationJsFunctions.RequestPermission, new InvalidOperationException("JS error"));
            var service = CreateService();
            
            // Act
            var result = await service.RequestPermissionAsync();
            
            // Assert
            Assert.False(result);
            Assert.False(service.IsNotificationPermitted);
        }

        [Fact]
        public async Task RefreshPermissionStateAsync_WhenGranted_UpdatesPermission()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, Constants.NotificationPermissions.Granted);
            var service = CreateService();
            
            // Act
            await service.RefreshPermissionStateAsync();
            
            // Assert
            Assert.True(service.IsNotificationPermitted);
        }

        [Fact]
        public async Task RefreshPermissionStateAsync_WhenDenied_UpdatesPermission()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.NotificationJsFunctions.RequestPermission, "denied");
            var service = CreateService();
            
            // Act
            await service.RefreshPermissionStateAsync();
            
            // Assert
            Assert.False(service.IsNotificationPermitted);
        }

        [Fact]
        public async Task RefreshPermissionStateAsync_WhenJsThrowsException_DoesNotUpdatePermission()
        {
            // Arrange
            SetupJsInvokeAsyncException<string>(Constants.NotificationJsFunctions.RequestPermission, new InvalidOperationException("JS error"));
            var service = CreateService();
            
            // Act
            await service.RefreshPermissionStateAsync();
            
            // Assert - Permission should remain false (default)
            Assert.False(service.IsNotificationPermitted);
        }
    }
}

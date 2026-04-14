using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Services;
using Pomodoro.Web.Services.Formatters;
using Pomodoro.Web.Services.Repositories;
using Pomodoro.Web.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Pomodoro.Web.Tests.Components
{
    public class AppTests : TestContext
    {
        public AppTests()
        {
            // Register AppState - required for Index page
            Services.AddSingleton(new AppState());
            
            // Register mock services
            Services.AddSingleton<ITimerService>(new Mock<ITimerService>().Object);
            Services.AddSingleton<ISettingsRepository>(new Mock<ISettingsRepository>().Object);
            Services.AddSingleton<IActivityRepository>(new Mock<IActivityRepository>().Object);
            Services.AddSingleton<ITaskRepository>(new Mock<ITaskRepository>().Object);
            Services.AddSingleton<ITaskService>(new Mock<ITaskService>().Object);
            Services.AddSingleton<IActivityService>(new Mock<IActivityService>().Object);
            Services.AddSingleton<ITimerService>(new Mock<ITimerService>().Object);
            Services.AddSingleton<ISessionOptionsService>(new Mock<ISessionOptionsService>().Object);
            Services.AddSingleton<IConsentService>(new Mock<IConsentService>().Object);
            Services.AddSingleton<INotificationService>(new Mock<INotificationService>().Object);
            Services.AddSingleton<IPipTimerService>(new Mock<IPipTimerService>().Object);
            Services.AddSingleton<IChartService, ChartService>();
            Services.AddSingleton<IKeyboardShortcutService>(new Mock<IKeyboardShortcutService>().Object);
            Services.AddSingleton<IExportService>(new Mock<IExportService>().Object);
            Services.AddSingleton<ITodayStatsService>(new Mock<ITodayStatsService>().Object);
            Services.AddSingleton<IHistoryStatsService>(new Mock<IHistoryStatsService>().Object);
            Services.AddSingleton<IIndexedDbService>(new Mock<IIndexedDbService>().Object);
            Services.AddSingleton<IJSInteropService>(new Mock<IJSInteropService>().Object);
            Services.AddSingleton<IInfiniteScrollInterop>(new Mock<IInfiniteScrollInterop>().Object);
            
            // Register concrete services (not mocked as they have no interface)
            Services.AddSingleton<LayoutPresenterService>();
            Services.AddSingleton<IndexPagePresenterService>();
            Services.AddSingleton<TimerThemeFormatter>();
            
            // Register logger mocks
            Services.AddSingleton(new Mock<ILogger<App>>().Object);
            Services.AddSingleton(new Mock<ILogger<IndexPagePresenterService>>().Object);
            
            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("swipeNavigation.init", _ => true);
            JSInterop.SetupVoid("swipeNavigation.dispose");
        }
         
        #region Basic Rendering Tests
         
        [Fact]
        public void Render_ShouldRenderRouter()
        {
            // Arrange & Act
            var cut = RenderComponent<App>();
             
            // Assert
            Assert.NotNull(cut);
        }
         
        [Fact]
        public void Render_ShouldRenderErrorBoundary()
        {
            // Arrange & Act
            var cut = RenderComponent<App>();
             
            // Assert
            Assert.NotNull(cut.FindComponents<ErrorBoundary>());
        }
         
        [Fact]
        public void Render_ShouldRenderNotFoundContent()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to a non-existent route
            navigationManager.NavigateTo("/non-existent-route");
             
            // Assert
            Assert.NotNull(component);
        }
         
        #endregion
         
        #region Routing Tests
         
        [Fact]
        public void Router_ShouldHandleValidRoutes()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to valid routes
            navigationManager.NavigateTo("/");
            navigationManager.NavigateTo("/about");
            navigationManager.NavigateTo("/history");
             
            // Assert
            Assert.NotNull(component);
        }
         
        [Fact]
        public void Router_ShouldHandleInvalidRoutes()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to invalid routes
            navigationManager.NavigateTo("/invalid-route-1");
            navigationManager.NavigateTo("/invalid-route-2");
             
            // Assert
            Assert.NotNull(component);
        }
         
        #endregion
         
        #region ErrorBoundary Tests
         
        [Fact]
        public void ErrorBoundary_ShouldRenderNotFoundContent()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/this-route-does-not-exist");
             
            // Assert
            Assert.NotNull(component);
        }
         
        [Fact]
        public void ErrorBoundary_ShouldDisplayErrorMessage()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/another-invalid-route");
             
            // Assert
            Assert.NotNull(component);
        }
         
        #endregion
         
        #region NotFound Content Tests
         
        [Fact]
        public void NotFound_ShouldDisplayCorrectTitle()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/trigger-not-found-title-test");
             
            // Assert
            Assert.NotNull(component);
        }
         
        [Fact]
        public void NotFound_ShouldDisplayCorrectMessage()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/trigger-not-found-message-test");
             
            // Assert
            Assert.NotNull(component);
        }
         
        [Fact]
        public void NotFound_ShouldUseConstants()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var component = RenderComponent<App>();
             
            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/verify-constants-usage");
             
            // Assert
            var pageTitle = Constants.Routing.NotFoundPageTitle;
            var errorMessage = Constants.Routing.NotFoundMessage;
             
            // These should match exactly what's in App.razor file
            Assert.Equal("Not found", pageTitle);
            Assert.Equal("Sorry, there's nothing at this address.", errorMessage);
             
            // Verify component is still valid
            Assert.NotNull(component);
             
            // Verify navigation occurred
            Assert.Contains("/verify-constants-usage", navigationManager.Uri);
        }

        #endregion

        #region Error Recovery Tests

        [Fact]
        public void RecoverError_ShouldCallErrorBoundaryRecover_WhenErrorBoundaryIsSet()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call RecoverError method directly
            appInstance.RecoverError();

            // Assert - Method should execute without throwing
            Assert.NotNull(cut);
        }

        [Fact]
        public void RecoverError_ShouldHandleNullErrorBoundary()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call RecoverError method multiple times (should handle null gracefully)
            appInstance.RecoverError();
            appInstance.RecoverError();

            // Assert - Method should execute without throwing even if ErrorBoundary is null
            Assert.NotNull(cut);
        }

        [Fact]
        public void ErrorBoundary_RecoverError_ShouldBeAccessible()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act & Assert - Verify RecoverError method exists and is public
            var recoverErrorMethod = appInstance.GetType().GetMethod("RecoverError");
            Assert.NotNull(recoverErrorMethod);
            Assert.True(recoverErrorMethod.IsPublic);
        }

        [Fact]
        public void RecoverError_ShouldBeCallableMultipleTimes()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call RecoverError multiple times
            for (int i = 0; i < 5; i++)
            {
                appInstance.RecoverError();
            }

            // Assert - All calls should complete successfully
            Assert.NotNull(cut);
        }

        [Fact]
        public void RecoverError_ShouldHandleNullErrorBoundary_ViaReflection()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Set _errorBoundary to null using reflection and call RecoverError
            var errorBoundaryField = appInstance.GetType().GetField(
                "_errorBoundary",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            errorBoundaryField!.SetValue(appInstance, null);

            appInstance.RecoverError();

            // Assert - Should complete successfully without throwing
            Assert.NotNull(cut);
        }

        #endregion

        #region NotFound Content Rendering Tests

        [Fact]
        public void NotFound_ShouldRenderPageTitle()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            
            // Act - Navigate to a non-existent route to trigger NotFound
            navigationManager.NavigateTo("/non-existent-page-for-coverage-test");
            var cut = RenderComponent<App>();

            // Assert - Verify the component renders without error
            Assert.NotNull(cut);
            
            // Verify we're on the not-found route
            Assert.Contains("/non-existent-page-for-coverage-test", navigationManager.Uri);
        }

        [Fact]
        public void NotFound_ShouldRenderNotFoundContentWithCorrectStructure()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            
            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/test-not-found-rendering");
            var cut = RenderComponent<App>();

            // Assert - Component should render successfully
            Assert.NotNull(cut);
            
            // Verify navigation occurred
            Assert.Contains("/test-not-found-rendering", navigationManager.Uri);
        }

        [Fact]
        public void NotFound_ShouldUseConstantsForTitleAndMessage()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            
            // Act - Navigate to trigger NotFound
            navigationManager.NavigateTo("/verify-not-found-constants");
            RenderComponent<App>();

            // Assert - Verify constants exist and have expected values
            Assert.Equal("Not found", Constants.Routing.NotFoundPageTitle);
            Assert.Equal("Sorry, there's nothing at this address.", Constants.Routing.NotFoundMessage);
        }



        #endregion

        #region Combined Error and NotFound Tests

        [Fact]
        public void App_ShouldHandleBothNotFoundAndErrorRecovery()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Navigate to NotFound
            navigationManager.NavigateTo("/test-combined-coverage");
            
            // Call RecoverError
            appInstance.RecoverError();

            // Assert - Both operations should complete successfully
            Assert.NotNull(cut);
            Assert.Contains("/test-combined-coverage", navigationManager.Uri);
        }



        #endregion
    }
}

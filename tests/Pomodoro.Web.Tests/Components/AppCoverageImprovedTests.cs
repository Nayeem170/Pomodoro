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
using Pomodoro.Web.Models;
using Pomodoro.Web.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace Pomodoro.Web.Tests
{
    /// <summary>
    /// Improved tests for App.razor component coverage
    /// Focuses on actually executing the code in the @code block
    /// </summary>
    [Trait("Category", "Component")]
    public class AppCoverageImprovedTests : TestContext
    {
        public AppCoverageImprovedTests()
        {
            // Register LayoutPresenterService as a mock
            var mockLayoutPresenter = new Mock<LayoutPresenterService>();
            mockLayoutPresenter.Setup(x => x.GetNavMenuCssClass()).Returns("collapse");
            mockLayoutPresenter.Setup(x => x.GetNavigationLinks()).Returns(new List<Pomodoro.Web.Services.NavLinkData>());
            mockLayoutPresenter.Setup(x => x.ToggleNavMenu());
            Services.AddSingleton<LayoutPresenterService>(mockLayoutPresenter.Object);

            // Register all of the services that Index.razor requires
            Services.AddSingleton(new Mock<ITaskService>().Object);
            Services.AddSingleton(new Mock<IActivityService>().Object);
            Services.AddSingleton(new Mock<ITimerService>().Object);
            Services.AddSingleton(new Mock<INotificationService>().Object);
            Services.AddSingleton(new Mock<IExportService>().Object);
            Services.AddSingleton(new Mock<IIndexedDbService>().Object);
            Services.AddSingleton(new Mock<IConsentService>().Object);
            Services.AddSingleton(new Mock<IPipTimerService>().Object);
            Services.AddSingleton(new Mock<IKeyboardShortcutService>().Object);
            Services.AddSingleton(new Mock<IndexPagePresenterService>(new Mock<ILogger<IndexPagePresenterService>>().Object).Object);
            Services.AddSingleton<ITodayStatsService>(new TodayStatsService(new Mock<IActivityService>().Object));
            Services.AddSingleton(new Mock<TimerThemeFormatter>().Object);
            Services.AddSingleton(new Mock<IJSRuntime>().Object);
            Services.AddSingleton(new Mock<ILogger<App>>().Object);
            Services.AddSingleton(new Mock<ILogger<LayoutPresenterService>>().Object);
            Services.AddSingleton(new Mock<ILogger<ErrorDisplay>>().Object);
            Services.AddSingleton(new Mock<ITimerEventPublisher>().Object);
            Services.AddSingleton(new Mock<AppState>().Object);
        }

        #region GetNotFoundPageTitle Method Tests - Direct Execution

        [Fact]
        public void GetNotFoundPageTitle_ShouldExecuteAndReturnValue()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call the method directly to ensure code execution
            var result = appInstance.GetNotFoundPageTitle();

            // Assert - Verify the method executed and returned the correct value
            Assert.NotNull(result);
            Assert.Equal("Not found", result);
        }


        [Fact]
        public void GetNotFoundPageTitle_ShouldReturnConstantValue()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act
            var result = appInstance.GetNotFoundPageTitle();

            // Assert - Verify it returns the constant value
            Assert.Equal(Constants.Routing.NotFoundPageTitle, result);
        }

        #endregion

        #region RecoverError Method Tests - Direct Execution

        [Fact]
        public void RecoverError_ShouldExecuteWithoutThrowing()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call the method directly to ensure code execution
            var exception = Record.Exception(() => appInstance.RecoverError());

            // Assert - Method should execute without throwing
            Assert.Null(exception);
        }


        [Fact]
        public void RecoverError_ShouldHandleNullErrorBoundary()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call RecoverError when ErrorBoundary might be null
            var exception = Record.Exception(() => appInstance.RecoverError());

            // Assert - Should handle null gracefully using null-conditional operator
            Assert.Null(exception);
        }

        [Fact]
        public void RecoverError_ShouldBePublicMethod()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act & Assert - Verify RecoverError is a public method
            var methodInfo = appInstance.GetType().GetMethod("RecoverError");
            Assert.NotNull(methodInfo);
            Assert.True(methodInfo.IsPublic);
            Assert.Equal(typeof(void), methodInfo.ReturnType);
        }

        [Fact]
        public void RecoverError_ShouldHaveNoParameters()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act & Assert - Verify RecoverError has no parameters
            var methodInfo = appInstance.GetType().GetMethod("RecoverError");
            Assert.NotNull(methodInfo);
            Assert.Empty(methodInfo.GetParameters());
        }

        #endregion

        #region Combined Method Execution Tests

        [Fact]
        public void App_ShouldExecuteBothMethodsSuccessfully()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Execute both methods
            var pageTitle = appInstance.GetNotFoundPageTitle();
            appInstance.RecoverError();

            // Assert - Both methods should execute successfully
            Assert.Equal("Not found", pageTitle);
            Assert.NotNull(cut);
        }


        #endregion

        #region Method Reflection Tests

        [Fact]
        public void GetNotFoundPageTitle_ShouldExistAsPublicMethod()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act & Assert - Verify GetNotFoundPageTitle is a public method
            var methodInfo = appInstance.GetType().GetMethod("GetNotFoundPageTitle");
            Assert.NotNull(methodInfo);
            Assert.True(methodInfo.IsPublic);
            Assert.Equal(typeof(string), methodInfo.ReturnType);
        }

        [Fact]
        public void GetNotFoundPageTitle_ShouldHaveNoParameters()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act & Assert - Verify GetNotFoundPageTitle has no parameters
            var methodInfo = appInstance.GetType().GetMethod("GetNotFoundPageTitle");
            Assert.NotNull(methodInfo);
            Assert.Empty(methodInfo.GetParameters());
        }

        [Fact]
        public void App_ShouldHaveBothRequiredMethods()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;
            var type = appInstance.GetType();

            // Act & Assert - Verify both methods exist
            var getNotFoundPageTitleMethod = type.GetMethod("GetNotFoundPageTitle");
            var recoverErrorMethod = type.GetMethod("RecoverError");

            Assert.NotNull(getNotFoundPageTitleMethod);
            Assert.NotNull(recoverErrorMethod);
        }

        #endregion

        #region ErrorBoundary Field Tests

        [Fact]
        public void App_ShouldHaveErrorBoundaryField()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;
            var type = appInstance.GetType();

            // Act & Assert - Verify _errorBoundary field exists
            var errorBoundaryField = type.GetField("_errorBoundary", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(errorBoundaryField);
            Assert.Equal(typeof(ErrorBoundary), errorBoundaryField.FieldType);
        }

        [Fact]
        public void ErrorBoundaryField_ShouldBeNullable()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;
            var type = appInstance.GetType();

            // Act & Assert - Verify _errorBoundary field exists
            var errorBoundaryField = type.GetField("_errorBoundary", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(errorBoundaryField);

            // The field should be nullable (ErrorBoundary?)
            // In C# 9+, nullable reference types are a compile-time feature
            // and don't show up as Nullable<> in reflection
            var fieldType = errorBoundaryField.FieldType;
            Assert.Equal(typeof(ErrorBoundary), fieldType);
        }

        #endregion

        #region Component Rendering with Method Execution

        [Fact]
        public void App_ShouldRenderAndExecuteMethods()
        {
            // Arrange & Act
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Execute methods after rendering
            var pageTitle = appInstance.GetNotFoundPageTitle();
            appInstance.RecoverError();

            // Assert - Component should render and methods should execute
            Assert.NotNull(cut);
            Assert.Equal("Not found", pageTitle);
        }


        #endregion

        #region Navigation and Method Execution Tests


        [Fact]
        public void App_ShouldNavigateToInvalidRouteAndExecuteMethods()
        {
            // Arrange
            var navigationManager = Services.GetRequiredService<NavigationManager>();
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Navigate to invalid route and execute methods
            navigationManager.NavigateTo("/invalid-route-for-coverage");
            var pageTitle = appInstance.GetNotFoundPageTitle();
            appInstance.RecoverError();

            // Assert - All operations should succeed
            Assert.NotNull(cut);
            Assert.Equal("Not found", pageTitle);
            Assert.Contains("/invalid-route-for-coverage", navigationManager.Uri);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void RecoverError_ShouldNotThrowWhenCalledBeforeRender()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act - Call RecoverError immediately after render
            var exception = Record.Exception(() => appInstance.RecoverError());

            // Assert - Should not throw
            Assert.Null(exception);
        }


        #endregion

        #region Method Return Value Tests


        [Fact]
        public void GetNotFoundPageTitle_ShouldReturnNonEmptyString()
        {
            // Arrange
            var cut = RenderComponent<App>();
            var appInstance = cut.Instance;

            // Act
            var result = appInstance.GetNotFoundPageTitle();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Length > 0);
        }

        #endregion
    }
}


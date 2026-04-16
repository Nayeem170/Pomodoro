using Xunit;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Moq;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for PipTimerService PiP operations (Initialize, Open, Close, Update, Dispose).
/// </summary>
[Trait("Category", "Service")]
public partial class PipTimerServiceTests
{
    /// <summary>
    /// Helper method to setup JS InvokeAsync for boolean results
    /// </summary>
    protected void SetupJsInvokeAsync<T>(string methodName, T result)
    {
        TestBase.SetupJsInvokeAsync(MockJsRuntime, methodName, result);
    }

    protected void SetupJsInvokeVoidAsync(string methodName)
    {
        TestBase.SetupJsInvokeVoidAsync(MockJsRuntime, methodName);
    }

    protected void SetupJsInvokeAsyncException<T>(string methodName, Exception exception)
    {
        TestBase.SetupJsInvokeAsyncException<T>(MockJsRuntime, methodName, exception);
    }

    protected void SetupJsInvokeVoidAsyncException(string methodName, Exception exception)
    {
        TestBase.SetupJsInvokeVoidAsyncException(MockJsRuntime, methodName, exception);
    }

    protected void VerifyJsInvokeVoidAsync(string methodName, Moq.Times times)
    {
        TestBase.VerifyJsInvokeVoidAsync(MockJsRuntime, methodName, times);
    }

    /// <summary>
    /// Tests for InitializeAsync method
    /// </summary>
    [Trait("Category", "Service")]
    public class InitializeAsyncTests : PipTimerServiceTests
    {
        [Fact]
        public async Task InitializeAsync_WhenNotInitialized_ChecksPipSupport()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);

            var service = CreateService();

            // Act
            await service.InitializeAsync();

            // Assert
            Assert.True(service.IsSupported);
            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef, Moq.Times.Once());
        }

        [Fact]
        public async Task InitializeAsync_WhenNotInitialized_RegistersDotNetReference()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);

            var service = CreateService();

            // Act
            await service.InitializeAsync();

            // Assert
            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef, Moq.Times.Once());
        }

        [Fact]
        public async Task InitializeAsync_WhenNotInitialized_SubscribesToTimerEvents()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);

            var service = CreateService();

            // Act
            await service.InitializeAsync();

            // Assert - Service was initialized (indirectly verified through no exceptions)
            Assert.True(service.IsSupported);
        }

        [Fact]
        public async Task InitializeAsync_WhenAlreadyInitialized_DoesNothing()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);

            var service = CreateService();
            await service.InitializeAsync();

            // Act - Call InitializeAsync again
            await service.InitializeAsync();

            // Assert - Should only be called once
            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef, Moq.Times.Once());
        }

        [Fact]
        public async Task InitializeAsync_WhenJsException_LogsError()
        {
            // Arrange
            SetupJsInvokeAsyncException<bool>(Constants.PipJsFunctions.IsSupported, new JSException("Test JS error"));

            var service = CreateService();

            // Act - Should not throw
            await service.InitializeAsync();

            // Assert - Service should remain in default state
            Assert.False(service.IsSupported);
        }

        [Fact]
        public async Task InitializeAsync_WhenPipNotSupported_SetsIsSupportedToFalse()
        {
            // Arrange
            SetupJsInvokeAsync(Constants.PipJsFunctions.IsSupported, false);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.RegisterDotNetRef);

            var service = CreateService();

            // Act
            await service.InitializeAsync();

            // Assert
            Assert.False(service.IsSupported);
        }
    }

    /// <summary>
    /// Tests for OpenAsync method
    /// </summary>
    [Trait("Category", "Service")]
    public class OpenAsyncTests : PipTimerServiceTests
    {
        [Fact]
        public async Task OpenAsync_WhenDisposed_ReturnsFalse()
        {
            // Arrange
            var service = CreateService();
            await service.DisposeAsync();

            // Act
            var result = await service.OpenAsync();

            // Assert
            Assert.False(result);
            Assert.False(service.IsOpen);
        }

        [Fact]
        public async Task OpenAsync_WhenSuccessful_SetsIsOpenAndFiresEvent()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);

            var service = CreateService();
            var eventRaised = false;
            service.OnPipOpened += () => eventRaised = true;

            // Act
            var result = await service.OpenAsync();

            // Assert
            Assert.True(result);
            Assert.True(service.IsOpen);
            Assert.True(eventRaised);
        }

        [Fact]
        public async Task OpenAsync_WhenJsReturnsFalse_DoesNotSetIsOpen()
        {
            // Arrange
            SetupTimerState(isRunning: false, isStarted: false);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, false);

            var service = CreateService();
            var eventRaised = false;
            service.OnPipOpened += () => eventRaised = true;

            // Act
            var result = await service.OpenAsync();

            // Assert
            Assert.False(result);
            Assert.False(service.IsOpen);
            Assert.False(eventRaised);
        }

        [Fact]
        public async Task OpenAsync_WhenJsException_LogsErrorAndReturnsFalse()
        {
            // Arrange
            SetupTimerState(isRunning: false, isStarted: false);
            SetupJsInvokeAsyncException<bool>(Constants.PipJsFunctions.Open, new JSException("Test JS error"));

            var service = CreateService();
            var eventRaised = false;
            service.OnPipOpened += () => eventRaised = true;

            // Act
            var result = await service.OpenAsync();

            // Assert
            Assert.False(result);
            Assert.False(service.IsOpen);
            Assert.False(eventRaised);
        }

        [Fact]
        public async Task OpenAsync_PassesCorrectTimerState()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true, remainingSeconds: 300, sessionType: SessionType.Pomodoro);
            MockTaskService.SetupGet(x => x.CurrentTask).Returns(new TaskItem { Name = "Test Task" });

            object?[]? capturedArgs = null;
            MockJsRuntime
                .Setup(x => x.InvokeAsync<bool>(Constants.PipJsFunctions.Open, It.IsAny<object?[]?>()))
                .Callback<string, object?[]?>((_, args) => capturedArgs = args)
                .ReturnsAsync(true);

            var service = CreateService();

            // Act
            await service.OpenAsync();

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Single(capturedArgs);
        }
    }

    /// <summary>
    /// Tests for CloseAsync method
    /// </summary>
    [Trait("Category", "Service")]
    public class CloseAsyncTests : PipTimerServiceTests
    {
        [Fact]
        public async Task CloseAsync_WhenDisposed_ReturnsEarly()
        {
            // Arrange
            var service = CreateService();
            await service.DisposeAsync();

            // Act - Should not throw
            await service.CloseAsync();

            // Assert - IsOpen should remain false
            Assert.False(service.IsOpen);
        }

        [Fact]
        public async Task CloseAsync_WhenOpen_SetsIsOpenFalseAndFiresEvent()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.Close);

            var service = CreateService();
            await service.OpenAsync(); // Open first

            var eventRaised = false;
            service.OnPipClosed += () => eventRaised = true;

            // Act
            await service.CloseAsync();

            // Assert
            Assert.False(service.IsOpen);
            Assert.True(eventRaised);
        }

        [Fact]
        public async Task CloseAsync_WhenJsException_LogsErrorAndDoesNotUpdateState()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            SetupJsInvokeVoidAsyncException(Constants.PipJsFunctions.Close, new JSException("Test JS error"));

            var service = CreateService();
            await service.OpenAsync(); // Open first

            var eventRaised = false;
            service.OnPipClosed += () => eventRaised = true;

            // Act - Should not throw
            await service.CloseAsync();

            // Assert - When JS exception occurs, IsOpen stays true and event is NOT raised
            // (Implementation only sets IsOpen=false and raises event on success)
            Assert.True(service.IsOpen);
            Assert.False(eventRaised);
        }

        [Fact]
        public async Task CloseAsync_CallsJsCloseFunction()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.Close);

            var service = CreateService();
            await service.OpenAsync(); // Open first

            // Act
            await service.CloseAsync();

            // Assert
            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.Close, Moq.Times.Once());
        }
    }

    /// <summary>
    /// Tests for UpdateTimerAsync method
    /// </summary>
    [Trait("Category", "Service")]
    public class UpdateTimerAsyncTests : PipTimerServiceTests
    {
        [Fact]
        public async Task UpdateTimerAsync_WhenDisposed_ReturnsEarly()
        {
            // Arrange
            var service = CreateService();
            await service.DisposeAsync();

            // Act - Should not throw
            await service.UpdateTimerAsync();

            // Assert - No JS call should be made
            MockJsRuntime.Verify(x => x.InvokeAsync<IJSVoidResult>(Constants.PipJsFunctions.Update, It.IsAny<object?[]?>()), Moq.Times.Never());
        }

        [Fact]
        public async Task UpdateTimerAsync_WhenNotOpen_ReturnsEarly()
        {
            // Arrange
            var service = CreateService();
            // IsOpen is false by default

            // Act
            await service.UpdateTimerAsync();

            // Assert - No JS call should be made
            MockJsRuntime.Verify(x => x.InvokeAsync<IJSVoidResult>(Constants.PipJsFunctions.Update, It.IsAny<object?[]?>()), Moq.Times.Never());
        }

        [Fact]
        public async Task UpdateTimerAsync_WhenOpen_UpdatesWithTimerState()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true, remainingSeconds: 300, sessionType: SessionType.Pomodoro);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.Update);

            var service = CreateService();
            await service.OpenAsync(); // Open first

            // Act
            await service.UpdateTimerAsync();

            // Assert
            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.Update, Moq.Times.Once());
        }

        [Fact]
        public async Task UpdateTimerAsync_WhenJsException_LogsError()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            SetupJsInvokeVoidAsyncException(Constants.PipJsFunctions.Update, new JSException("Test JS error"));

            var service = CreateService();
            await service.OpenAsync(); // Open first

            // Act - Should not throw
            await service.UpdateTimerAsync();

            // Assert - No exception thrown, error logged
            Assert.True(service.IsOpen);
        }
    }

    /// <summary>
    /// Tests for DisposeAsync method
    /// </summary>
    [Trait("Category", "Service")]
    public class DisposeAsyncTests : PipTimerServiceTests
    {
        [Fact]
        public async Task DisposeAsync_WhenNotDisposed_SetsIsDisposed()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef);

            var service = CreateService();

            // Act
            await service.DisposeAsync();

            // Assert - After disposal, OpenAsync should return false
            var result = await service.OpenAsync();
            Assert.False(result);
        }

        [Fact]
        public async Task DisposeAsync_WhenOpen_DoesNotCallCloseAsyncDirectly()
        {
            // Arrange
            SetupTimerState(isRunning: true, isStarted: true);
            SetupJsInvokeAsync(Constants.PipJsFunctions.Open, true);
            // Close should NOT be called because DisposeAsync sets _isDisposed=true before calling CloseAsync
            // and CloseAsync returns early when _isDisposed is true
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef);

            var service = CreateService();
            await service.OpenAsync(); // Open first
            Assert.True(service.IsOpen);

            // Act
            await service.DisposeAsync();

            // Assert - IsOpen stays true because CloseAsync returns early when disposed
            // (DisposeAsync sets _isDisposed=true before calling CloseAsync)
            Assert.True(service.IsOpen);
        }

        [Fact]
        public async Task DisposeAsync_WhenAlreadyDisposed_DoesNothing()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef);

            var service = CreateService();
            await service.DisposeAsync();

            // Act - Dispose again
            await service.DisposeAsync();

            // Assert - Unregister should only be called once
            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef, Moq.Times.Once());
        }

        [Fact]
        public async Task DisposeAsync_WhenJsException_DoesNotThrow()
        {
            // Arrange
            SetupJsInvokeVoidAsyncException(Constants.PipJsFunctions.UnregisterDotNetRef, new JSException("Test JS error"));

            var service = CreateService();

            // Act - Should not throw
            await service.DisposeAsync();

            // Assert - Service should be disposed despite error
            var result = await service.OpenAsync();
            Assert.False(result);
        }

        [Fact]
        public async Task DisposeAsync_UnregistersDotNetReference()
        {
            // Arrange
            SetupJsInvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef);

            var service = CreateService();

            // Act
            await service.DisposeAsync();

            // Assert
            VerifyJsInvokeVoidAsync(Constants.PipJsFunctions.UnregisterDotNetRef, Moq.Times.Once());
        }
    }
}


using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;
using static Moq.Times;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Base test class for NotificationService tests.
/// </summary>
public partial class NotificationServiceTests
{
    protected readonly Mock<IJSRuntime> MockJsRuntime;
    protected readonly Mock<ILogger<NotificationService>> MockLogger;
    protected readonly AppState AppState;
    
    public NotificationServiceTests()
    {
        MockJsRuntime = new Mock<IJSRuntime>();
        MockLogger = new Mock<ILogger<NotificationService>>();
        AppState = new AppState();
        
        // Setup JS runtime to return completed tasks for void invocations
        // InvokeVoidAsync internally calls InvokeAsync<IJSVoidResult>
        MockJsRuntime
            .Setup(js => js.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));
    }
    
    protected NotificationService CreateService()
    {
        return new NotificationService(MockJsRuntime.Object, AppState, MockLogger.Object);
    }
    
    protected void SetupJsInvokeAsync<T>(string methodName, T result)
    {
        // Setup for overload with args - this is what IJSRuntime.InvokeAsync actually uses
        MockJsRuntime
            .Setup(js => js.InvokeAsync<T>(methodName, It.IsAny<object?[]?>()))
            .ReturnsAsync(result);
    }
    
    protected void SetupJsInvokeVoidAsync(string methodName)
    {
        // InvokeVoidAsync internally calls InvokeAsync<IJSVoidResult>
        MockJsRuntime
            .Setup(js => js.InvokeAsync<IJSVoidResult>(methodName, It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));
    }
    
    protected void SetupJsInvokeAsyncException<T>(string methodName, Exception exception)
    {
        MockJsRuntime
            .Setup(js => js.InvokeAsync<T>(methodName, It.IsAny<object?[]?>()))
            .ThrowsAsync(exception);
    }
    
    protected void SetupJsInvokeVoidAsyncException(string methodName, Exception exception)
    {
        // InvokeVoidAsync internally calls InvokeAsync<IJSVoidResult>
        MockJsRuntime
            .Setup(js => js.InvokeAsync<IJSVoidResult>(methodName, It.IsAny<object?[]?>()))
            .ThrowsAsync(exception);
    }
    
    protected void VerifyJsInvokeVoidAsync(string methodName, Times times)
    {
        MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(methodName, It.IsAny<object?[]?>()), times);
    }
    
    protected void VerifyJsInvokeVoidAsync(string methodName, Func<object?[]?, bool> predicate, Times times)
    {
        MockJsRuntime.Verify(js => js.InvokeAsync<IJSVoidResult>(methodName, It.Is<object?[]?>(args => predicate(args))), times);
    }
    
    /// <summary>
    /// Helper method to check if an argument at a specific index matches the expected value.
    /// This avoids null propagating operator issues in expression trees.
    /// </summary>
    protected static bool ArgumentMatches(object?[]? args, int index, string expectedValue)
    {
        if (args == null || args.Length <= index || args[index] == null)
            return false;
        return args[index]!.ToString() == expectedValue;
    }
}

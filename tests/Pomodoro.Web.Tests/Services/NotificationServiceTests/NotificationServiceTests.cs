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
    
    protected void VerifyJsInvokeVoidAsync(string methodName, Times times)
    {
        TestBase.VerifyJsInvokeVoidAsync(MockJsRuntime, methodName, times);
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

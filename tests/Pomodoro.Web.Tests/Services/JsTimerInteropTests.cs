using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

[Trait("Category", "Service")]
public class JsTimerInteropTests
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<ILogger<JsTimerInterop>> _mockLogger;

    public JsTimerInteropTests()
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockLogger = new Mock<ILogger<JsTimerInterop>>();
    }

    private JsTimerInterop CreateService()
    {
        return new JsTimerInterop(_mockJsRuntime.Object, _mockLogger.Object);
    }

    #region StartAsync

    [Fact]
    public async Task StartAsync_InvokesUnlockAudioAndTimerStart()
    {
        SetupVoidInvoke(Constants.NotificationJsFunctions.UnlockAudio);
        SetupVoidInvoke(Constants.JsFunctions.TimerStart);

        var service = CreateService();
        var dotNetRef = DotNetObjectReference.Create(new object());
        await service.StartAsync(dotNetRef);

        _mockJsRuntime.Verify(
            x => x.InvokeAsync<IJSVoidResult>(Constants.NotificationJsFunctions.UnlockAudio, It.IsAny<object?[]?>()),
            Times.Once);
        _mockJsRuntime.Verify(
            x => x.InvokeAsync<IJSVoidResult>(Constants.JsFunctions.TimerStart, It.IsAny<object?[]?>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenAudioUnlockFails_StillStartsTimer()
    {
        SetupVoidInvokeException(Constants.NotificationJsFunctions.UnlockAudio, new JSException("Audio unlock failed"));
        SetupVoidInvoke(Constants.JsFunctions.TimerStart);

        var service = CreateService();
        var dotNetRef = DotNetObjectReference.Create(new object());
        await service.StartAsync(dotNetRef);

        _mockJsRuntime.Verify(
            x => x.InvokeAsync<IJSVoidResult>(Constants.JsFunctions.TimerStart, It.IsAny<object?[]?>()),
            Times.Once);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenTimerStartFails_RetriesOnce()
    {
        SetupVoidInvoke(Constants.NotificationJsFunctions.UnlockAudio);
        var callCount = 0;
        SetupVoidInvokeConditional(Constants.JsFunctions.TimerStart, () =>
        {
            callCount++;
            if (callCount == 1)
                throw new JSException("Timer start failed");
        });

        var service = CreateService();
        var dotNetRef = DotNetObjectReference.Create(new object());
        await service.StartAsync(dotNetRef);

        Assert.Equal(2, callCount);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenBothAttemptsFail_LogsError()
    {
        SetupVoidInvoke(Constants.NotificationJsFunctions.UnlockAudio);
        SetupVoidInvokeException(Constants.JsFunctions.TimerStart, new JSException("Timer start failed"));

        var service = CreateService();
        var dotNetRef = DotNetObjectReference.Create(new object());
        await service.StartAsync(dotNetRef);

        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region StopAsync

    [Fact]
    public async Task StopAsync_InvokesTimerStop()
    {
        SetupVoidInvoke(Constants.JsFunctions.TimerStop);

        var service = CreateService();
        await service.StopAsync();

        _mockJsRuntime.Verify(
            x => x.InvokeAsync<IJSVoidResult>(Constants.JsFunctions.TimerStop, It.IsAny<object?[]?>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_WhenFails_LogsWarning()
    {
        SetupVoidInvokeException(Constants.JsFunctions.TimerStop, new JSException("Timer stop failed"));

        var service = CreateService();
        await service.StopAsync();

        _mockLogger.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helpers

    private void SetupVoidInvoke(string identifier)
    {
        _mockJsRuntime
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.IsAny<CancellationToken>(), It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));
        _mockJsRuntime
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.IsAny<object?[]?>()))
            .Returns(new ValueTask<IJSVoidResult>(default(IJSVoidResult)!));
    }

    private void SetupVoidInvokeException(string identifier, Exception exception)
    {
        _mockJsRuntime
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.IsAny<CancellationToken>(), It.IsAny<object?[]?>()))
            .ThrowsAsync(exception);
        _mockJsRuntime
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.IsAny<object?[]?>()))
            .ThrowsAsync(exception);
    }

    private void SetupVoidInvokeConditional(string identifier, Action onInvoke)
    {
        _mockJsRuntime
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.IsAny<CancellationToken>(), It.IsAny<object?[]?>()))
            .Returns(() =>
            {
                onInvoke();
                return new ValueTask<IJSVoidResult>(default(IJSVoidResult)!);
            });
        _mockJsRuntime
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.IsAny<object?[]?>()))
            .Returns(() =>
            {
                onInvoke();
                return new ValueTask<IJSVoidResult>(default(IJSVoidResult)!);
            });
    }

    #endregion
}

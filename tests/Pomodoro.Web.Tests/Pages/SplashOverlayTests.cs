using System.Reflection;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Pomodoro.Web;
using Pomodoro.Web.Models;
using Pomodoro.Web.Pages;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Pages;

[Trait("Category", "Page")]
public class SplashOverlayTests : TestHelper
{
    public SplashOverlayTests()
    {
        NotificationServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        PipTimerServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        TaskServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        TaskServiceMock.SetupGet(x => x.Tasks).Returns(new List<TaskItem>());
        TaskServiceMock.SetupGet(x => x.AllTasks).Returns(new List<TaskItem>());
        TaskServiceMock.SetupGet(x => x.CurrentTaskId).Returns((Guid?)null);
        TaskServiceMock.SetupGet(x => x.CurrentTask).Returns((TaskItem?)null);
        TimerServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        TimerServiceMock.SetupGet(x => x.RemainingTime).Returns(TimeSpan.FromMinutes(25));
        TimerServiceMock.SetupGet(x => x.CurrentSessionType).Returns(SessionType.Pomodoro);
        TimerServiceMock.SetupGet(x => x.IsRunning).Returns(false);
        TimerServiceMock.SetupGet(x => x.IsPaused).Returns(false);
        TimerServiceMock.SetupGet(x => x.IsStarted).Returns(false);
        ConsentServiceMock.Setup(x => x.SelectOptionAsync(It.IsAny<SessionType>())).Returns(Task.CompletedTask);
        ActivityServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        ActivityServiceMock.Setup(x => x.GetActivitiesPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivitiesForDate(It.IsAny<DateTime>()))
            .Returns(new List<ActivityRecord>());
        ActivityServiceMock.Setup(x => x.GetActivityCountAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);
        ActivityServiceMock.Setup(x => x.GetDailyFocusMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        ActivityServiceMock.Setup(x => x.GetDailyBreakMinutes(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new Dictionary<DateTime, int>());
        StatisticsServiceMock.Setup<Task<WeeklyStats?>>(x => x.GetWeeklyStatsAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((WeeklyStats?)null);
    }

    [Fact]
    public async Task Index_AfterLoad_HidesSplashOverlayOnce()
    {
        // Arrange
        var js = new RecordingJsRuntime();
        Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();
        var method = OnAfterRenderMethod(typeof(IndexBase));

        // Act
        await InvokeOnAfterRender(method, cut.Instance);
        await InvokeOnAfterRender(method, cut.Instance);

        // Assert
        js.Identifiers.Count(i => i == Constants.JsFunctions.HideSplash).Should().Be(1);
    }

    [Fact]
    public async Task Index_HideSplashJsFailure_DoesNotFaultPage()
    {
        // Arrange
        var js = new ThrowingJsRuntime();
        Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderComponent<Pomodoro.Web.Pages.Index>();

        // Act
        await InvokeOnAfterRender(OnAfterRenderMethod(typeof(IndexBase)), cut.Instance);

        // Assert
        cut.Instance.ErrorMessage.Should().BeNull();
        js.CallCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task History_AfterLoad_HidesSplashOverlayOnce()
    {
        // Arrange
        var js = new RecordingJsRuntime();
        Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();
        var method = OnAfterRenderMethod(typeof(HistoryBase));

        // Act
        await InvokeOnAfterRender(method, cut.Instance);
        await InvokeOnAfterRender(method, cut.Instance);

        // Assert
        js.Identifiers.Count(i => i == Constants.JsFunctions.HideSplash).Should().Be(1);
    }

    [Fact]
    public async Task History_HideSplashJsFailure_DoesNotFaultPage()
    {
        // Arrange
        var js = new ThrowingJsRuntime();
        Services.AddSingleton<IJSRuntime>(js);
        var cut = RenderComponent<Pomodoro.Web.Pages.History>();

        // Act
        await InvokeOnAfterRender(OnAfterRenderMethod(typeof(HistoryBase)), cut.Instance);

        // Assert
        cut.Find(".hist-body").Should().NotBeNull();
        js.CallCount.Should().BeGreaterThanOrEqualTo(1);
    }

    private static MethodInfo OnAfterRenderMethod(Type type)
        => type.GetMethod("OnAfterRenderAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static async Task InvokeOnAfterRender(MethodInfo method, object instance)
        => await (Task)method.Invoke(instance, [false])!;

    private sealed class RecordingJsRuntime : IJSRuntime
    {
        public List<string> Identifiers { get; } = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Identifiers.Add(identifier);
            return ValueTask.FromResult<TValue>(default!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Identifiers.Add(identifier);
            return ValueTask.FromResult<TValue>(default!);
        }

        public ValueTask InvokeVoidAsync(string identifier, object?[]? args)
        {
            Identifiers.Add(identifier);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingJsRuntime : IJSRuntime
    {
        public int CallCount { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            CallCount++;
            throw new JSDisconnectedException("disconnected");
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            CallCount++;
            throw new JSDisconnectedException("disconnected");
        }

        public ValueTask InvokeVoidAsync(string identifier, object?[]? args)
        {
            CallCount++;
            throw new JSDisconnectedException("disconnected");
        }
    }
}

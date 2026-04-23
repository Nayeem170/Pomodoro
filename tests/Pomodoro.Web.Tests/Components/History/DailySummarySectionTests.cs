using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.History;

[Trait("Category", "Component")]
public class DailySummarySectionTests : TestContext
{
    public DailySummarySectionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        var commonServices = TestBase.CreateCommonServiceCollection();
        foreach (var service in commonServices)
        {
            Services.Add(service);
        }
        Services.AddSingleton(new Mock<ILogger<HistoryPagePresenterService>>().Object);
        Services.AddSingleton<HistoryPagePresenterService>();
    }

    [Fact]
    public void Renders_WithStats_DisplaysValues()
    {
        var stats = new DailyStatsSummary
        {
            FocusMinutes = 90,
            PomodoroCount = 5,
            TasksWorkedOn = 3
        };

        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, stats));

        Assert.Contains("Pomodoros", cut.Markup);
        Assert.Contains("Focus time", cut.Markup);
        Assert.Contains("Tasks done", cut.Markup);
    }

    [Fact]
    public void Renders_WithNullStats_DisplaysZeroDefaults()
    {
        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, (DailyStatsSummary?)null));

        var statValues = cut.FindAll(".sv");
        Assert.Equal(4, statValues.Count);

        foreach (var statValue in statValues)
        {
            Assert.Contains("0", statValue.TextContent);
        }
    }

    [Fact]
    public void FormatFocusTime_WithNullService_ReturnsRawMinutes()
    {
        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 100 }));

        var property = cut.Instance.GetType().GetProperty(
            "HistoryPagePresenterService",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        property!.SetValue(cut.Instance, null);

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 200 }));

        Assert.Contains("200", cut.Markup);
    }

    [Fact]
    public void FormatFocusTime_WithNonNullService_CallsServiceMethod()
    {
        var stats = new DailyStatsSummary
        {
            FocusMinutes = 150,
            PomodoroCount = 8,
            TasksWorkedOn = 6
        };

        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, stats));

        Assert.Contains("150", cut.Markup);
        Assert.Contains("8", cut.Markup);
        Assert.Contains("6", cut.Markup);
    }

    [Fact]
    public void Renders_WithNullServiceViaStateChange_DisplaysRawMinutes()
    {
        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 50 }));

        var property = cut.Instance.GetType().GetProperty(
            "HistoryPagePresenterService",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        property!.SetValue(cut.Instance, null);

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 85 }));

        Assert.Contains("85", cut.Markup);
    }

    [Fact]
    public async Task Renders_WithNullServiceViaInvokeAsync_CallsFormatFocusTime()
    {
        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 45 }));

        var property = cut.Instance.GetType().GetProperty(
            "HistoryPagePresenterService",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        property!.SetValue(cut.Instance, null);

        await cut.InvokeAsync(() =>
        {
            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 110 }));
        });

        Assert.Contains("110", cut.Markup);
    }

    [Fact]
    public void FormatFocusTime_DirectlyInvoked_CallsBothBranches()
    {
        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 77 }));

        var result = cut.Instance.FormatFocusTime(88);
        Assert.Equal("88", result);
    }

    [Fact]
    public void FormatFocusTime_WithNullService_ViaDirectMethodCall_ReturnsRawMinutes()
    {
        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, new DailyStatsSummary { FocusMinutes = 50 }));

        var property = cut.Instance.GetType().GetProperty(
            "HistoryPagePresenterService",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        property!.SetValue(cut.Instance, null);

        var result = cut.Instance.FormatFocusTime(123);
        Assert.Equal("123", result);
    }


    [Fact]
    public void Renders_WithDefaultStats_DisplaysZeroValues()
    {
        var stats = new DailyStatsSummary();

        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, stats));

        var statValues = cut.FindAll(".sv");
        Assert.Equal(4, statValues.Count);
        Assert.Contains("0", statValues[0].TextContent);
        Assert.Contains("0", statValues[1].TextContent);
        Assert.Contains("0", statValues[2].TextContent);
        Assert.Contains("0", statValues[3].TextContent);
    }

    [Fact]
    public void Renders_WithNonNullStats_DisplaysExactValues()
    {
        var stats = new DailyStatsSummary
        {
            FocusMinutes = 120,
            PomodoroCount = 7,
            TasksWorkedOn = 5
        };

        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, stats));

        Assert.Contains("120", cut.Markup);
        Assert.Contains("7", cut.Markup);
        Assert.Contains("5", cut.Markup);
    }

    [Fact]
    public void Renders_WithBreakMinutes_DisplaysBreakTime()
    {
        var stats = new DailyStatsSummary
        {
            FocusMinutes = 120,
            PomodoroCount = 7,
            TasksWorkedOn = 5,
            BreakMinutes = 30
        };

        var cut = RenderComponent<DailySummarySection>(parameters => parameters
            .Add(p => p.CurrentStats, stats));

        Assert.Contains("Break time", cut.Markup);
    }
}

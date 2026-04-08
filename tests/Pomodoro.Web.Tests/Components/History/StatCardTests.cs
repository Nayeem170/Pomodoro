using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Services.Formatters;
using Xunit;

namespace Pomodoro.Web.Tests.Components.History;

/// <summary>
/// Tests for StatCard component.
/// Tests rendering with different parameter values.
/// </summary>
public class StatCardTests : TestContext
{
    public StatCardTests()
    {
        // Add JSInterop for Blazor
        JSInterop.Mode = JSRuntimeMode.Loose;
        // Register formatter service
        Services.AddScoped<StatCardFormatter>();
    }

    #region Rendering Tests

    [Fact]
    public void StatCard_WithDefaultValues_RendersCorrectly()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>();

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("stat-card", cut.Markup);
    }

    [Fact]
    public void StatCard_WithCustomIcon_RendersIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Icon, "clock"));

        // Assert
        Assert.Contains("stat-card", cut.Markup);
        Assert.Contains("clock", cut.Markup);
    }

    [Fact]
    public void StatCard_WithCustomValue_RendersValue()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Value, "42"));

        // Assert
        Assert.Contains("stat-card", cut.Markup);
        Assert.Contains("42", cut.Markup);
    }

    [Fact]
    public void StatCard_WithCustomLabel_RendersLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Label, "Sessions"));

        // Assert
        Assert.Contains("stat-card", cut.Markup);
        Assert.Contains("Sessions", cut.Markup);
    }

    [Fact]
    public void StatCard_WithAllParameters_RendersAll()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Icon, "clock")
            .Add(p => p.Value, "42")
            .Add(p => p.Label, "Sessions"));

        // Assert
        Assert.NotNull(cut.Markup);
        Assert.Contains("stat-card", cut.Markup);
        Assert.Contains("clock", cut.Markup);
        Assert.Contains("42", cut.Markup);
        Assert.Contains("Sessions", cut.Markup);
    }

    [Fact]
    public void StatCard_WithEmptyIcon_RendersWithoutIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Icon, ""));

        // Assert
        Assert.Contains("stat-card", cut.Markup);
        Assert.DoesNotContain("clock", cut.Markup);
    }

    [Fact]
    public void StatCard_WithEmptyValue_RendersWithoutValue()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Value, ""));

        // Assert
        Assert.Contains("stat-card", cut.Markup);
        Assert.DoesNotContain("42", cut.Markup);
    }

    [Fact]
    public void StatCard_WithEmptyLabel_RendersWithoutLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Label, ""));

        // Assert
        Assert.Contains("stat-card", cut.Markup);
        Assert.DoesNotContain("Sessions", cut.Markup);
    }

    #endregion

    #region Code-Behind Method Tests

    [Fact]
    public void GetFormattedValue_WithValue_ReturnsValue()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Value, "42"));

        // Act
        var result = cut.Instance.GetFormattedValue();

        // Assert
        Assert.Equal("42", result);
    }

    [Fact]
    public void GetFormattedValue_WithEmptyValue_ReturnsZero()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Value, ""));

        // Act
        var result = cut.Instance.GetFormattedValue();

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetFormattedLabel_WithLabel_ReturnsLabel()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Label, "Sessions"));

        // Act
        var result = cut.Instance.GetFormattedLabel();

        // Assert
        Assert.Equal("Sessions", result);
    }

    [Fact]
    public void GetFormattedLabel_WithEmptyLabel_ReturnsNA()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Label, ""));

        // Act
        var result = cut.Instance.GetFormattedLabel();

        // Assert
        Assert.Equal("N/A", result);
    }

    [Fact]
    public void GetFormattedIcon_WithIcon_ReturnsIcon()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Icon, "clock"));

        // Act
        var result = cut.Instance.GetFormattedIcon();

        // Assert
        Assert.Equal("clock", result);
    }

    [Fact]
    public void GetFormattedIcon_WithEmptyIcon_ReturnsDefaultIcon()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Icon, ""));

        // Act
        var result = cut.Instance.GetFormattedIcon();

        // Assert
        Assert.Equal("📊", result);
    }

    [Fact]
    public void HasRequiredData_WithAllData_ReturnsTrue()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Icon, "clock")
            .Add(p => p.Value, "42")
            .Add(p => p.Label, "Sessions"));

        // Act
        var result = cut.Instance.HasRequiredData();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasRequiredData_WithMissingData_ReturnsFalse()
    {
        // Arrange
        var cut = RenderComponent<StatCard>(parameters => parameters
            .Add(p => p.Icon, "clock")
            .Add(p => p.Value, ""));

        // Act
        var result = cut.Instance.HasRequiredData();

        // Assert
        Assert.False(result);
    }

    #endregion
}

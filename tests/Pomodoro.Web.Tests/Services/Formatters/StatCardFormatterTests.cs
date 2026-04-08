using Xunit;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Tests.Services.Formatters;

/// <summary>
/// Tests for StatCardFormatter service
/// </summary>
public class StatCardFormatterTests
{
    private readonly StatCardFormatter _formatter = new();

    #region GetFormattedValue Tests

    [Fact]
    public void GetFormattedValue_WhenValueIsNull_ReturnsZero()
    {
        // Arrange
        string? value = null;

        // Act
        var result = _formatter.GetFormattedValue(value);

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetFormattedValue_WhenValueIsEmpty_ReturnsZero()
    {
        // Arrange
        string value = string.Empty;

        // Act
        var result = _formatter.GetFormattedValue(value);

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetFormattedValue_WhenValueIsWhitespace_ReturnsZero()
    {
        // Arrange
        string value = "   ";

        // Act
        var result = _formatter.GetFormattedValue(value);

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetFormattedValue_WhenValueHasContent_ReturnsValue()
    {
        // Arrange
        string value = "42";

        // Act
        var result = _formatter.GetFormattedValue(value);

        // Assert
        Assert.Equal("42", result);
    }

    [Fact]
    public void GetFormattedValue_WhenValueIsZero_ReturnsZero()
    {
        // Arrange
        string value = "0";

        // Act
        var result = _formatter.GetFormattedValue(value);

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void GetFormattedValue_WhenValueIsLargeNumber_ReturnsValue()
    {
        // Arrange
        string value = "9999";

        // Act
        var result = _formatter.GetFormattedValue(value);

        // Assert
        Assert.Equal("9999", result);
    }

    #endregion

    #region GetFormattedLabel Tests

    [Fact]
    public void GetFormattedLabel_WhenLabelIsNull_ReturnsNA()
    {
        // Arrange
        string? label = null;

        // Act
        var result = _formatter.GetFormattedLabel(label);

        // Assert
        Assert.Equal("N/A", result);
    }

    [Fact]
    public void GetFormattedLabel_WhenLabelIsEmpty_ReturnsNA()
    {
        // Arrange
        string label = string.Empty;

        // Act
        var result = _formatter.GetFormattedLabel(label);

        // Assert
        Assert.Equal("N/A", result);
    }

    [Fact]
    public void GetFormattedLabel_WhenLabelIsWhitespace_ReturnsNA()
    {
        // Arrange
        string label = "   ";

        // Act
        var result = _formatter.GetFormattedLabel(label);

        // Assert
        Assert.Equal("N/A", result);
    }

    [Fact]
    public void GetFormattedLabel_WhenLabelHasContent_ReturnsLabel()
    {
        // Arrange
        string label = "Focus Time";

        // Act
        var result = _formatter.GetFormattedLabel(label);

        // Assert
        Assert.Equal("Focus Time", result);
    }

    [Fact]
    public void GetFormattedLabel_WhenLabelIsSingleCharacter_ReturnsLabel()
    {
        // Arrange
        string label = "A";

        // Act
        var result = _formatter.GetFormattedLabel(label);

        // Assert
        Assert.Equal("A", result);
    }

    #endregion

    #region GetFormattedIcon Tests

    [Fact]
    public void GetFormattedIcon_WhenIconIsNull_ReturnsDefaultIcon()
    {
        // Arrange
        string? icon = null;

        // Act
        var result = _formatter.GetFormattedIcon(icon);

        // Assert
        Assert.Equal("📊", result);
    }

    [Fact]
    public void GetFormattedIcon_WhenIconIsEmpty_ReturnsDefaultIcon()
    {
        // Arrange
        string icon = string.Empty;

        // Act
        var result = _formatter.GetFormattedIcon(icon);

        // Assert
        Assert.Equal("📊", result);
    }

    [Fact]
    public void GetFormattedIcon_WhenIconIsWhitespace_ReturnsDefaultIcon()
    {
        // Arrange
        string icon = "   ";

        // Act
        var result = _formatter.GetFormattedIcon(icon);

        // Assert
        Assert.Equal("📊", result);
    }

    [Fact]
    public void GetFormattedIcon_WhenIconHasContent_ReturnsIcon()
    {
        // Arrange
        string icon = "🎯";

        // Act
        var result = _formatter.GetFormattedIcon(icon);

        // Assert
        Assert.Equal("🎯", result);
    }

    [Fact]
    public void GetFormattedIcon_WhenIconIsText_ReturnsIcon()
    {
        // Arrange
        string icon = "star";

        // Act
        var result = _formatter.GetFormattedIcon(icon);

        // Assert
        Assert.Equal("star", result);
    }

    #endregion

    #region HasRequiredData Tests

    [Fact]
    public void HasRequiredData_WhenAllParametersAreNull_ReturnsFalse()
    {
        // Arrange
        string? icon = null;
        string? value = null;
        string? label = null;

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenAllParametersAreEmpty_ReturnsFalse()
    {
        // Arrange
        string icon = string.Empty;
        string value = string.Empty;
        string label = string.Empty;

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenOnlyIconHasValue_ReturnsFalse()
    {
        // Arrange
        string icon = "📊";
        string value = string.Empty;
        string label = string.Empty;

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenOnlyValueHasValue_ReturnsFalse()
    {
        // Arrange
        string icon = string.Empty;
        string value = "42";
        string label = string.Empty;

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenOnlyLabelHasValue_ReturnsFalse()
    {
        // Arrange
        string icon = string.Empty;
        string value = string.Empty;
        string label = "Focus Time";

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenIconAndValueHaveValue_ReturnsFalse()
    {
        // Arrange
        string icon = "📊";
        string value = "42";
        string label = string.Empty;

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenIconAndLabelHaveValue_ReturnsFalse()
    {
        // Arrange
        string icon = "📊";
        string value = string.Empty;
        string label = "Focus Time";

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenValueAndLabelHaveValue_ReturnsFalse()
    {
        // Arrange
        string icon = string.Empty;
        string value = "42";
        string label = "Focus Time";

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequiredData_WhenAllParametersHaveValue_ReturnsTrue()
    {
        // Arrange
        string icon = "📊";
        string value = "42";
        string label = "Focus Time";

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasRequiredData_WhenAllParametersHaveWhitespaceOnly_ReturnsFalse()
    {
        // Arrange
        string icon = "   ";
        string value = "   ";
        string label = "   ";

        // Act
        var result = _formatter.HasRequiredData(icon, value, label);

        // Assert
        Assert.False(result);
    }

    #endregion
}

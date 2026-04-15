using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models.ImportResultTests;

/// <summary>
/// Tests for ImportResult model
/// </summary>
[Trait("Category", "Model")]
public class ImportResultTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesSuccessToFalse()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void Constructor_InitializesErrorMessageToNull()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Constructor_InitializesActivitiesImportedToZero()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.Equal(0, result.ActivitiesImported);
    }

    [Fact]
    public void Constructor_InitializesActivitiesSkippedToZero()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.Equal(0, result.ActivitiesSkipped);
    }

    [Fact]
    public void Constructor_InitializesTasksImportedToZero()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.Equal(0, result.TasksImported);
    }

    [Fact]
    public void Constructor_InitializesTasksSkippedToZero()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.Equal(0, result.TasksSkipped);
    }

    [Fact]
    public void Constructor_InitializesSettingsImportedToFalse()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.False(result.SettingsImported);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void SuccessProperty_CanBeSet()
    {
        // Arrange
        var result = new ImportResult();

        // Act
        result.Success = true;

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void ErrorMessageProperty_CanBeSet()
    {
        // Arrange
        var result = new ImportResult();
        var expectedMessage = "Import failed";

        // Act
        result.ErrorMessage = expectedMessage;

        // Assert
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    [Fact]
    public void ActivitiesImportedProperty_CanBeSet()
    {
        // Arrange
        var result = new ImportResult();
        var expectedCount = 10;

        // Act
        result.ActivitiesImported = expectedCount;

        // Assert
        Assert.Equal(expectedCount, result.ActivitiesImported);
    }

    [Fact]
    public void ActivitiesSkippedProperty_CanBeSet()
    {
        // Arrange
        var result = new ImportResult();
        var expectedCount = 5;

        // Act
        result.ActivitiesSkipped = expectedCount;

        // Assert
        Assert.Equal(expectedCount, result.ActivitiesSkipped);
    }

    [Fact]
    public void TasksImportedProperty_CanBeSet()
    {
        // Arrange
        var result = new ImportResult();
        var expectedCount = 20;

        // Act
        result.TasksImported = expectedCount;

        // Assert
        Assert.Equal(expectedCount, result.TasksImported);
    }

    [Fact]
    public void TasksSkippedProperty_CanBeSet()
    {
        // Arrange
        var result = new ImportResult();
        var expectedCount = 3;

        // Act
        result.TasksSkipped = expectedCount;

        // Assert
        Assert.Equal(expectedCount, result.TasksSkipped);
    }

    [Fact]
    public void SettingsImportedProperty_CanBeSet()
    {
        // Arrange
        var result = new ImportResult();

        // Act
        result.SettingsImported = true;

        // Assert
        Assert.True(result.SettingsImported);
    }

    #endregion

    #region Computed Properties Tests

    [Fact]
    public void TotalImportedProperty_ReturnsSumOfActivitiesAndTasks()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesImported = 10,
            TasksImported = 20
        };

        // Act
        var total = result.TotalImported;

        // Assert
        Assert.Equal(30, total);
    }

    [Fact]
    public void TotalImportedProperty_WithZeroActivities_ReturnsTasksCount()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesImported = 0,
            TasksImported = 15
        };

        // Act
        var total = result.TotalImported;

        // Assert
        Assert.Equal(15, total);
    }

    [Fact]
    public void TotalImportedProperty_WithZeroTasks_ReturnsActivitiesCount()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesImported = 25,
            TasksImported = 0
        };

        // Act
        var total = result.TotalImported;

        // Assert
        Assert.Equal(25, total);
    }

    [Fact]
    public void TotalImportedProperty_WithBothZero_ReturnsZero()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesImported = 0,
            TasksImported = 0
        };

        // Act
        var total = result.TotalImported;

        // Assert
        Assert.Equal(0, total);
    }

    [Fact]
    public void TotalSkippedProperty_ReturnsSumOfActivitiesAndTasksSkipped()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesSkipped = 5,
            TasksSkipped = 3
        };

        // Act
        var total = result.TotalSkipped;

        // Assert
        Assert.Equal(8, total);
    }

    [Fact]
    public void TotalSkippedProperty_WithZeroActivitiesSkipped_ReturnsTasksSkippedCount()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesSkipped = 0,
            TasksSkipped = 7
        };

        // Act
        var total = result.TotalSkipped;

        // Assert
        Assert.Equal(7, total);
    }

    [Fact]
    public void TotalSkippedProperty_WithZeroTasksSkipped_ReturnsActivitiesSkippedCount()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesSkipped = 12,
            TasksSkipped = 0
        };

        // Act
        var total = result.TotalSkipped;

        // Assert
        Assert.Equal(12, total);
    }

    [Fact]
    public void TotalSkippedProperty_WithBothZero_ReturnsZero()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesSkipped = 0,
            TasksSkipped = 0
        };

        // Act
        var total = result.TotalSkipped;

        // Assert
        Assert.Equal(0, total);
    }

    #endregion

    #region Static Factory Method Tests

    [Fact]
    public void Failed_CreatesResultWithSuccessFalse()
    {
        // Act
        var result = ImportResult.Failed("Error message");

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void Failed_SetsErrorMessage()
    {
        // Arrange
        var expectedMessage = "Invalid JSON format";

        // Act
        var result = ImportResult.Failed(expectedMessage);

        // Assert
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    [Fact]
    public void Failed_InitializesCountsToZero()
    {
        // Act
        var result = ImportResult.Failed("Error");

        // Assert
        Assert.Equal(0, result.ActivitiesImported);
        Assert.Equal(0, result.ActivitiesSkipped);
        Assert.Equal(0, result.TasksImported);
        Assert.Equal(0, result.TasksSkipped);
        Assert.False(result.SettingsImported);
    }

    [Fact]
    public void Failed_WithEmptyErrorMessage_SetsEmptyString()
    {
        // Act
        var result = ImportResult.Failed(string.Empty);

        // Assert
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    [Fact]
    public void Succeeded_CreatesResultWithSuccessTrue()
    {
        // Act
        var result = ImportResult.Succeeded(10, 5, 20, 3, true);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void Succeeded_SetsActivitiesImported()
    {
        // Arrange
        var expectedCount = 15;

        // Act
        var result = ImportResult.Succeeded(expectedCount, 0, 0, 0, false);

        // Assert
        Assert.Equal(expectedCount, result.ActivitiesImported);
    }

    [Fact]
    public void Succeeded_SetsActivitiesSkipped()
    {
        // Arrange
        var expectedCount = 8;

        // Act
        var result = ImportResult.Succeeded(0, expectedCount, 0, 0, false);

        // Assert
        Assert.Equal(expectedCount, result.ActivitiesSkipped);
    }

    [Fact]
    public void Succeeded_SetsTasksImported()
    {
        // Arrange
        var expectedCount = 25;

        // Act
        var result = ImportResult.Succeeded(0, 0, expectedCount, 0, false);

        // Assert
        Assert.Equal(expectedCount, result.TasksImported);
    }

    [Fact]
    public void Succeeded_SetsTasksSkipped()
    {
        // Arrange
        var expectedCount = 4;

        // Act
        var result = ImportResult.Succeeded(0, 0, 0, expectedCount, false);

        // Assert
        Assert.Equal(expectedCount, result.TasksSkipped);
    }

    [Fact]
    public void Succeeded_SetsSettingsImported()
    {
        // Act
        var result = ImportResult.Succeeded(0, 0, 0, 0, true);

        // Assert
        Assert.True(result.SettingsImported);
    }

    [Fact]
    public void Succeeded_SetsSettingsImportedToFalse()
    {
        // Act
        var result = ImportResult.Succeeded(0, 0, 0, 0, false);

        // Assert
        Assert.False(result.SettingsImported);
    }

    [Fact]
    public void Succeeded_InitializesErrorMessageToNull()
    {
        // Act
        var result = ImportResult.Succeeded(10, 5, 20, 3, true);

        // Assert
        Assert.Null(result.ErrorMessage);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TotalImportedProperty_HandlesLargeNumbers()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesImported = 10000,
            TasksImported = 5000
        };

        // Act
        var total = result.TotalImported;

        // Assert
        Assert.Equal(15000, total);
    }

    [Fact]
    public void TotalSkippedProperty_HandlesLargeNumbers()
    {
        // Arrange
        var result = new ImportResult
        {
            ActivitiesSkipped = 1000,
            TasksSkipped = 500
        };

        // Act
        var total = result.TotalSkipped;

        // Assert
        Assert.Equal(1500, total);
    }

    [Fact]
    public void ErrorMessageProperty_CanBeNull()
    {
        // Arrange
        var result = new ImportResult();

        // Act
        result.ErrorMessage = null;

        // Assert
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Succeeded_WithAllZeroValues_CreatesValidResult()
    {
        // Act
        var result = ImportResult.Succeeded(0, 0, 0, 0, false);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.TotalImported);
        Assert.Equal(0, result.TotalSkipped);
        Assert.False(result.SettingsImported);
    }

    [Fact]
    public void Succeeded_WithMixedValues_CalculatesCorrectTotals()
    {
        // Arrange
        var activitiesImported = 100;
        var activitiesSkipped = 10;
        var tasksImported = 50;
        var tasksSkipped = 5;

        // Act
        var result = ImportResult.Succeeded(activitiesImported, activitiesSkipped, tasksImported, tasksSkipped, true);

        // Assert
        Assert.Equal(150, result.TotalImported);
        Assert.Equal(15, result.TotalSkipped);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullObject_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var result = new ImportResult
        {
            Success = true,
            ErrorMessage = null,
            ActivitiesImported = 100,
            ActivitiesSkipped = 10,
            TasksImported = 50,
            TasksSkipped = 5,
            SettingsImported = true
        };

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(100, result.ActivitiesImported);
        Assert.Equal(10, result.ActivitiesSkipped);
        Assert.Equal(50, result.TasksImported);
        Assert.Equal(5, result.TasksSkipped);
        Assert.True(result.SettingsImported);
        Assert.Equal(150, result.TotalImported);
        Assert.Equal(15, result.TotalSkipped);
    }

    [Fact]
    public void MultipleImportResults_CanBeCreatedIndependently()
    {
        // Arrange
        var result1 = ImportResult.Succeeded(10, 2, 5, 1, true);
        var result2 = ImportResult.Succeeded(20, 3, 10, 2, false);

        // Act & Assert
        Assert.NotEqual(result1.ActivitiesImported, result2.ActivitiesImported);
        Assert.NotEqual(result1.TotalImported, result2.TotalImported);
        Assert.NotEqual(result1.SettingsImported, result2.SettingsImported);
    }

    [Fact]
    public void FailedAndSucceeded_CoexistIndependently()
    {
        // Arrange
        var failedResult = ImportResult.Failed("Error message");
        var succeededResult = ImportResult.Succeeded(10, 5, 20, 3, true);

        // Act & Assert
        Assert.False(failedResult.Success);
        Assert.NotNull(failedResult.ErrorMessage);
        Assert.True(succeededResult.Success);
        Assert.Null(succeededResult.ErrorMessage);
    }

    #endregion
}


using Pomodoro.Web.Models;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

public class ExportKeysTests
{
    [Fact]
    public void ActivityKey_Constructor_SetsAllProperties()
    {
        var completedAt = new DateTime(2024, 6, 15, 14, 30, 0);
        var key = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Test Task");

        Assert.Equal(SessionType.Pomodoro, key.Type);
        Assert.Equal(completedAt, key.CompletedAt);
        Assert.Equal(25, key.DurationMinutes);
        Assert.Equal("Test Task", key.TaskName);
    }

    [Fact]
    public void ActivityKey_WithNullTaskName_SetsCorrectly()
    {
        var key = new ActivityKey(SessionType.ShortBreak, DateTime.Now, 5, null);

        Assert.Null(key.TaskName);
        Assert.Equal(SessionType.ShortBreak, key.Type);
    }

    [Fact]
    public void ActivityKey_Equality_SameValuesAreEqual()
    {
        var completedAt = new DateTime(2024, 1, 1, 10, 0, 0);
        var key1 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task");
        var key2 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task");

        Assert.Equal(key1, key2);
        Assert.True(key1 == key2);
    }

    [Fact]
    public void ActivityKey_Equality_DifferentType_NotEqual()
    {
        var completedAt = new DateTime(2024, 1, 1, 10, 0, 0);
        var key1 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task");
        var key2 = new ActivityKey(SessionType.ShortBreak, completedAt, 25, "Task");

        Assert.NotEqual(key1, key2);
        Assert.True(key1 != key2);
    }

    [Fact]
    public void ActivityKey_Equality_DifferentCompletedAt_NotEqual()
    {
        var key1 = new ActivityKey(SessionType.Pomodoro, new DateTime(2024, 1, 1), 25, "Task");
        var key2 = new ActivityKey(SessionType.Pomodoro, new DateTime(2024, 1, 2), 25, "Task");

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ActivityKey_Equality_DifferentDuration_NotEqual()
    {
        var completedAt = new DateTime(2024, 1, 1);
        var key1 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task");
        var key2 = new ActivityKey(SessionType.Pomodoro, completedAt, 30, "Task");

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ActivityKey_Equality_DifferentTaskName_NotEqual()
    {
        var completedAt = new DateTime(2024, 1, 1);
        var key1 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task A");
        var key2 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task B");

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ActivityKey_Equality_NullVsNonNullTaskName_NotEqual()
    {
        var completedAt = new DateTime(2024, 1, 1);
        var key1 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, null);
        var key2 = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task");

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void TaskKey_Constructor_SetsAllProperties()
    {
        var createdAt = new DateTime(2024, 6, 15);
        var key = new TaskKey("My Task", createdAt);

        Assert.Equal("My Task", key.Name);
        Assert.Equal(createdAt, key.CreatedAt);
    }

    [Fact]
    public void TaskKey_Equality_SameValuesAreEqual()
    {
        var createdAt = new DateTime(2024, 1, 1);
        var key1 = new TaskKey("Task", createdAt);
        var key2 = new TaskKey("Task", createdAt);

        Assert.Equal(key1, key2);
        Assert.True(key1 == key2);
    }

    [Fact]
    public void TaskKey_Equality_DifferentName_NotEqual()
    {
        var createdAt = new DateTime(2024, 1, 1);
        var key1 = new TaskKey("Task A", createdAt);
        var key2 = new TaskKey("Task B", createdAt);

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void TaskKey_Equality_DifferentCreatedAt_NotEqual()
    {
        var key1 = new TaskKey("Task", new DateTime(2024, 1, 1));
        var key2 = new TaskKey("Task", new DateTime(2024, 1, 2));

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ActivityKey_CanBeUsedAsDictionaryKey()
    {
        var completedAt = new DateTime(2024, 1, 1);
        var key = new ActivityKey(SessionType.Pomodoro, completedAt, 25, "Task");
        var dict = new Dictionary<ActivityKey, int> { { key, 1 } };

        Assert.True(dict.ContainsKey(key));
        Assert.Equal(1, dict[key]);
    }

    [Fact]
    public void TaskKey_CanBeUsedAsDictionaryKey()
    {
        var createdAt = new DateTime(2024, 1, 1);
        var key = new TaskKey("Task", createdAt);
        var dict = new Dictionary<TaskKey, int> { { key, 1 } };

        Assert.True(dict.ContainsKey(key));
        Assert.Equal(1, dict[key]);
    }
}

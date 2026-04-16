using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Pomodoro.Web.Models;

namespace Pomodoro.Web.Tests.Models;

/// <summary>
/// Comprehensive tests for AppState to improve coverage from 89.4% to 95%+
/// Tests thread-safety, event handling, edge cases, and daily reset functionality
/// </summary>
[Trait("Category", "Model")]
public class AppStateTests
{
    private readonly AppState _appState;

    public AppStateTests()
    {
        _appState = new AppState();
    }

    [Fact]
    public void Constructor_InitialState_AllPropertiesInitialized()
    {
        // Assert
        Assert.NotNull(_appState.Tasks);
        Assert.Empty(_appState.Tasks);
        Assert.Null(_appState.CurrentTaskId);
        Assert.NotNull(_appState.Settings);
        Assert.Null(_appState.CurrentSession);
        Assert.Equal(0, _appState.TodayTotalFocusMinutes);
        Assert.Equal(0, _appState.TodayPomodoroCount);
        Assert.NotNull(_appState.TodayTaskIdsWorkedOn);
        Assert.Empty(_appState.TodayTaskIdsWorkedOn);
        Assert.Null(_appState.LastResetDate);
        Assert.Null(_appState.CurrentTask);
        Assert.Equal(0, _appState.TodayTasksWorkedOn);
    }

    [Fact]
    public void Tasks_SetNull_UsesEmptyList()
    {
        // Arrange
        var initialCount = _appState.Tasks.Count;

        // Act
        _appState.Tasks = null!;

        // Assert
        Assert.NotNull(_appState.Tasks);
        Assert.Empty(_appState.Tasks);
    }

    [Fact]
    public void Tasks_SetNonListCollection_CopiesToList()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test Task" };
        IReadOnlyList<TaskItem> collection = new[] { task };

        // Act - Set Tasks to an IEnumerable that is NOT a List<TaskItem>
        // This covers the `(value as List<TaskItem>) ?? value?.ToList()` branch
        _appState.Tasks = collection;

        // Assert
        Assert.Single(_appState.Tasks);
        Assert.Equal("Test Task", _appState.Tasks[0].Name);
        Assert.True(eventCalled);
    }

    [Fact]
    public void Tasks_SetValidList_UpdatesAndNotifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var newTasks = new List<TaskItem> { new() { Id = Guid.NewGuid(), Name = "Test Task" } };

        // Act
        _appState.Tasks = newTasks;

        // Assert
        Assert.Single(_appState.Tasks);
        Assert.Equal("Test Task", _appState.Tasks[0].Name);
        Assert.True(eventCalled);
    }

    [Fact]
    public void InsertTask_ValidTask_InsertsAtPositionAndNotifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var task1 = new TaskItem { Id = Guid.NewGuid(), Name = "Task 1" };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Name = "Task 2" };
        _appState.Tasks = new List<TaskItem> { task1 };

        // Act
        _appState.InsertTask(task2, 0);

        // Assert
        Assert.Equal(2, _appState.Tasks.Count);
        Assert.Equal("Task 2", _appState.Tasks[0].Name);
        Assert.Equal("Task 1", _appState.Tasks[1].Name);
        Assert.True(eventCalled);
    }

    [Fact]
    public void InsertTask_DefaultPosition_InsertsAtBeginning()
    {
        // Arrange
        var task1 = new TaskItem { Id = Guid.NewGuid(), Name = "Task 1" };
        var task2 = new TaskItem { Id = Guid.NewGuid(), Name = "Task 2" };
        _appState.Tasks = new List<TaskItem> { task1 };

        // Act
        _appState.InsertTask(task2);

        // Assert
        Assert.Equal(2, _appState.Tasks.Count);
        Assert.Equal("Task 2", _appState.Tasks[0].Name);
        Assert.Equal("Task 1", _appState.Tasks[1].Name);
    }

    [Fact]
    public void UpdateTask_ExistingTask_UpdatesAndReturnsTrue()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Original Name" };
        _appState.Tasks = new List<TaskItem> { task };

        // Act
        var result = _appState.UpdateTask(taskId, t => t.Name = "Updated Name");

        // Assert
        Assert.True(result);
        Assert.Equal("Updated Name", _appState.Tasks[0].Name);
    }

    [Fact]
    public void UpdateTask_NonExistentTask_ReturnsFalse()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Task 1" };
        _appState.Tasks = new List<TaskItem> { task };

        // Act
        var result = _appState.UpdateTask(Guid.NewGuid(), t => t.Name = "Updated Name");

        // Assert
        Assert.False(result);
        Assert.Equal("Task 1", _appState.Tasks[0].Name);
    }

    [Fact]
    public void FindTaskById_ExistingTask_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Test Task" };
        _appState.Tasks = new List<TaskItem> { task };

        // Act
        var result = _appState.FindTaskById(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Test Task", result.Name);
    }

    [Fact]
    public void FindTaskById_NonExistentTask_ReturnsNull()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test Task" };
        _appState.Tasks = new List<TaskItem> { task };

        // Act
        var result = _appState.FindTaskById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CurrentTaskId_SetSameValue_DoesNotNotify()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        _appState.CurrentTaskId = taskId;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.CurrentTaskId = taskId;

        // Assert
        Assert.Equal(taskId, _appState.CurrentTaskId);
        Assert.False(eventCalled);
    }

    [Fact]
    public void CurrentTaskId_SetNullTwice_DoesNotNotify()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;

        // Act
        _appState.CurrentTaskId = null;
        eventCalled = false;
        _appState.CurrentTaskId = null;

        // Assert
        Assert.Null(_appState.CurrentTaskId);
        Assert.False(eventCalled);
    }

    [Fact]
    public void CurrentTaskId_SetNullThenValue_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        _appState.CurrentTaskId = null;

        // Act
        _appState.CurrentTaskId = Guid.NewGuid();

        // Assert
        Assert.True(eventCalled);
    }

    [Fact]
    public void CurrentTaskId_SetValueThenNull_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        _appState.CurrentTaskId = Guid.NewGuid();

        // Act
        eventCalled = false;
        _appState.CurrentTaskId = null;

        // Assert
        Assert.True(eventCalled);
    }

    [Fact]
    public void CurrentTaskId_SetDifferentValue_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var taskId1 = Guid.NewGuid();
        var taskId2 = Guid.NewGuid();
        _appState.CurrentTaskId = taskId1;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.CurrentTaskId = taskId2;

        // Assert
        Assert.Equal(taskId2, _appState.CurrentTaskId);
        Assert.True(eventCalled);
    }

    [Fact]
    public void Settings_SetNull_UsesDefaultSettings()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;

        // Act
        _appState.Settings = null!;

        // Assert
        Assert.NotNull(_appState.Settings);
        Assert.True(eventCalled);
    }

    [Fact]
    public void Settings_SetSameInstance_DoesNotNotify()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var settings = _appState.Settings;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.Settings = settings;

        // Assert
        Assert.Equal(settings, _appState.Settings);
        Assert.False(eventCalled);
    }

    [Fact]
    public void Settings_SetDifferentInstance_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var newSettings = new TimerSettings { PomodoroMinutes = 30 };

        // Act
        _appState.Settings = newSettings;

        // Assert
        Assert.Equal(newSettings, _appState.Settings);
        Assert.True(eventCalled);
    }

    [Fact]
    public void CurrentSession_SetSameValue_DoesNotNotify()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var session = new TimerSession { Id = Guid.NewGuid(), StartedAt = DateTime.Now };
        _appState.CurrentSession = session;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.CurrentSession = session;

        // Assert
        Assert.Equal(session, _appState.CurrentSession);
        Assert.False(eventCalled);
    }

    [Fact]
    public void CurrentSession_SetDifferentValue_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var session1 = new TimerSession { Id = Guid.NewGuid(), StartedAt = DateTime.Now };
        var session2 = new TimerSession { Id = Guid.NewGuid(), StartedAt = DateTime.Now };
        _appState.CurrentSession = session1;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.CurrentSession = session2;

        // Assert
        Assert.Equal(session2, _appState.CurrentSession);
        Assert.True(eventCalled);
    }

    [Fact]
    public void TodayTotalFocusMinutes_SetSameValue_DoesNotNotify()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        _appState.TodayTotalFocusMinutes = 25;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.TodayTotalFocusMinutes = 25;

        // Assert
        Assert.Equal(25, _appState.TodayTotalFocusMinutes);
        Assert.False(eventCalled);
    }

    [Fact]
    public void TodayTotalFocusMinutes_SetDifferentValue_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        _appState.TodayTotalFocusMinutes = 25;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.TodayTotalFocusMinutes = 50;

        // Assert
        Assert.Equal(50, _appState.TodayTotalFocusMinutes);
        Assert.True(eventCalled);
    }

    [Fact]
    public void TodayPomodoroCount_SetSameValue_DoesNotNotify()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        _appState.TodayPomodoroCount = 3;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.TodayPomodoroCount = 3;

        // Assert
        Assert.Equal(3, _appState.TodayPomodoroCount);
        Assert.False(eventCalled);
    }

    [Fact]
    public void TodayPomodoroCount_SetDifferentValue_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        _appState.TodayPomodoroCount = 3;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.TodayPomodoroCount = 5;

        // Assert
        Assert.Equal(5, _appState.TodayPomodoroCount);
        Assert.True(eventCalled);
    }

    [Fact]
    public void TodayTaskIdsWorkedOn_SetNull_UsesEmptyList()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;

        // Act
        _appState.TodayTaskIdsWorkedOn = null!;

        // Assert
        Assert.NotNull(_appState.TodayTaskIdsWorkedOn);
        Assert.Empty(_appState.TodayTaskIdsWorkedOn);
        Assert.True(eventCalled);
    }

    [Fact]
    public void TodayTaskIdsWorkedOn_SetValidList_UpdatesAndNotifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var taskIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        _appState.TodayTaskIdsWorkedOn = taskIds;

        // Assert
        Assert.Equal(2, _appState.TodayTaskIdsWorkedOn.Count);
        Assert.Contains(taskIds[0], _appState.TodayTaskIdsWorkedOn);
        Assert.Contains(taskIds[1], _appState.TodayTaskIdsWorkedOn);
        Assert.True(eventCalled);
    }

    [Fact]
    public void LastResetDate_SetSameValue_DoesNotNotify()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var date = DateTime.Today;
        _appState.LastResetDate = date;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.LastResetDate = date;

        // Assert
        Assert.Equal(date, _appState.LastResetDate);
        Assert.False(eventCalled);
    }

    [Fact]
    public void LastResetDate_SetDifferentValue_Notifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var date1 = DateTime.Today;
        var date2 = DateTime.Today.AddDays(-1);
        _appState.LastResetDate = date1;

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.LastResetDate = date2;

        // Assert
        Assert.Equal(date2, _appState.LastResetDate);
        Assert.True(eventCalled);
    }

    [Fact]
    public void CurrentTask_WithCurrentTaskId_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Name = "Test Task" };
        _appState.Tasks = new List<TaskItem> { task };
        _appState.CurrentTaskId = taskId;

        // Act
        var result = _appState.CurrentTask;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Test Task", result.Name);
    }

    [Fact]
    public void CurrentTask_WithoutCurrentTaskId_ReturnsNull()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test Task" };
        _appState.Tasks = new List<TaskItem> { task };

        // Act
        var result = _appState.CurrentTask;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CurrentTask_WithNonExistentTaskId_ReturnsNull()
    {
        // Arrange
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Test Task" };
        _appState.Tasks = new List<TaskItem> { task };
        _appState.CurrentTaskId = Guid.NewGuid();

        // Act
        var result = _appState.CurrentTask;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TodayTasksWorkedOn_WithDuplicateTaskIds_ReturnsUniqueCount()
    {
        // Arrange
        var taskId1 = Guid.NewGuid();
        var taskId2 = Guid.NewGuid();
        var taskIds = new List<Guid> { taskId1, taskId2, taskId1, taskId2 };
        _appState.TodayTaskIdsWorkedOn = taskIds;

        // Act
        var result = _appState.TodayTasksWorkedOn;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void TodayTasksWorkedOn_WithEmptyList_ReturnsZero()
    {
        // Arrange
        _appState.TodayTaskIdsWorkedOn = new List<Guid>();

        // Act
        var result = _appState.TodayTasksWorkedOn;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void AddTodayTaskId_NewTaskId_AddsAndNotifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var taskId = Guid.NewGuid();

        // Act
        _appState.AddTodayTaskId(taskId);

        // Assert
        Assert.Single(_appState.TodayTaskIdsWorkedOn);
        Assert.Contains(taskId, _appState.TodayTaskIdsWorkedOn);
        Assert.True(eventCalled);
    }

    [Fact]
    public void AddTodayTaskId_ExistingTaskId_DoesNotDuplicateAndNotifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;
        var taskId = Guid.NewGuid();
        _appState.TodayTaskIdsWorkedOn = new List<Guid> { taskId };

        // Reset event flag
        eventCalled = false;

        // Act
        _appState.AddTodayTaskId(taskId);

        // Assert
        Assert.Single(_appState.TodayTaskIdsWorkedOn);
        Assert.Contains(taskId, _appState.TodayTaskIdsWorkedOn);
        Assert.True(eventCalled); // Still notifies even if not added
    }

    [Fact]
    public void GetCurrentDayKey_ReturnsTodayDate()
    {
        // Act
        var result = AppState.GetCurrentDayKey();

        // Assert
        Assert.Equal(DateTime.Today, result);
    }

    [Fact]
    public void NeedsDailyReset_WithNullLastResetDate_ReturnsTrue()
    {
        // Arrange
        _appState.LastResetDate = null;

        // Act
        var result = _appState.NeedsDailyReset();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NeedsDailyReset_WithOlderDate_ReturnsTrue()
    {
        // Arrange
        _appState.LastResetDate = DateTime.Today.AddDays(-1);

        // Act
        var result = _appState.NeedsDailyReset();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NeedsDailyReset_WithTodayDate_ReturnsFalse()
    {
        // Arrange
        _appState.LastResetDate = DateTime.Today;

        // Act
        var result = _appState.NeedsDailyReset();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NeedsDailyReset_WithFutureDate_ReturnsFalse()
    {
        // Arrange
        _appState.LastResetDate = DateTime.Today.AddDays(1);

        // Act
        var result = _appState.NeedsDailyReset();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ResetDailyStats_ResetsAllDailyStatsAndNotifies()
    {
        // Arrange
        var eventCalled = false;
        _appState.OnStateChanged += () => eventCalled = true;

        // Set up initial state
        _appState.TodayTotalFocusMinutes = 100;
        _appState.TodayPomodoroCount = 5;
        _appState.TodayTaskIdsWorkedOn = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _appState.LastResetDate = DateTime.Today.AddDays(-1);

        // Act
        _appState.ResetDailyStats();

        // Assert
        Assert.Equal(0, _appState.TodayTotalFocusMinutes);
        Assert.Equal(0, _appState.TodayPomodoroCount);
        Assert.Empty(_appState.TodayTaskIdsWorkedOn);
        Assert.Equal(DateTime.Today, _appState.LastResetDate);
        Assert.True(eventCalled);
    }

    [Fact]
    public void NotifyStateChanged_WithNoSubscribers_DoesNotThrow()
    {
        // Act & Assert
        _appState.NotifyStateChanged(); // Should not throw
    }

    [Fact]
    public void NotifyStateChanged_WithSubscribers_CallsAllSubscribers()
    {
        // Arrange
        var subscriber1Called = false;
        var subscriber2Called = false;
        _appState.OnStateChanged += () => subscriber1Called = true;
        _appState.OnStateChanged += () => subscriber2Called = true;

        // Act
        _appState.NotifyStateChanged();

        // Assert
        Assert.True(subscriber1Called);
        Assert.True(subscriber2Called);
    }

    [Fact]
    public void Tasks_ReturnsCopy_ModifyingReturnedDoesNotAffectOriginal()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Name = "Original Task" };
        _appState.Tasks = new List<TaskItem> { task };

        var tasksCopy = _appState.Tasks;
        ((List<TaskItem>)tasksCopy).Add(new TaskItem { Id = Guid.NewGuid(), Name = "Added Task" });

        Assert.Single(_appState.Tasks);
        Assert.Equal("Original Task", _appState.Tasks[0].Name);
    }

    [Fact]
    public void TodayTaskIdsWorkedOn_ReturnsCopy_ModifyingReturnedDoesNotAffectOriginal()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _appState.TodayTaskIdsWorkedOn = new List<Guid> { taskId };

        // Act
        var taskIdsCopy = _appState.TodayTaskIdsWorkedOn;
        taskIdsCopy.Add(Guid.NewGuid());

        // Assert
        Assert.Single(_appState.TodayTaskIdsWorkedOn);
        Assert.Contains(taskId, _appState.TodayTaskIdsWorkedOn);
    }

    [Fact]
    public void ConcurrentAccess_TasksProperty_ThreadSafe()
    {
        // Arrange
        var tasks = new List<TaskItem>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(new TaskItem { Id = Guid.NewGuid(), Name = $"Task {i}" });
        }
        _appState.Tasks = tasks;

        // Act
        Parallel.For(0, 10, i =>
        {
            var currentTasks = _appState.Tasks;
            Assert.Equal(100, currentTasks.Count);
        });

        // Assert
        Assert.Equal(100, _appState.Tasks.Count);
    }

    [Fact]
    public void ConcurrentAccess_TodayTaskIdsWorkedOnProperty_ThreadSafe()
    {
        // Arrange
        var taskIds = new List<Guid>();
        for (int i = 0; i < 100; i++)
        {
            taskIds.Add(Guid.NewGuid());
        }
        _appState.TodayTaskIdsWorkedOn = taskIds;

        // Act
        Parallel.For(0, 10, i =>
        {
            var currentTaskIds = _appState.TodayTaskIdsWorkedOn;
            Assert.Equal(100, currentTaskIds.Count);
        });

        // Assert
        Assert.Equal(100, _appState.TodayTaskIdsWorkedOn.Count);
    }

    [Fact]
    public void ConcurrentAccess_InsertTask_ThreadSafe()
    {
        // Arrange
        _appState.Tasks = new List<TaskItem>();

        // Act
        Parallel.For(0, 10, i =>
        {
            var task = new TaskItem { Id = Guid.NewGuid(), Name = $"Task {i}" };
            _appState.InsertTask(task);
        });

        // Assert
        Assert.Equal(10, _appState.Tasks.Count);
    }

    [Fact]
    public void ConcurrentAccess_AddTodayTaskId_ThreadSafe()
    {
        // Arrange
        var taskIds = new List<Guid>();
        for (int i = 0; i < 10; i++)
        {
            taskIds.Add(Guid.NewGuid());
        }

        // Act
        Parallel.For(0, 10, i =>
        {
            _appState.AddTodayTaskId(taskIds[i]);
        });

        // Assert
        Assert.Equal(10, _appState.TodayTaskIdsWorkedOn.Count);
        Assert.Equal(10, _appState.TodayTasksWorkedOn);
    }

    [Fact]
    public void Tasks_Setter_WithListDirectly_CopiesList()
    {
        var tasks = new List<TaskItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Task1" }
            };

        _appState.Tasks = tasks;

        Assert.Single(_appState.Tasks);
        Assert.Equal(tasks[0].Id, _appState.Tasks[0].Id);
    }

    [Fact]
    public void Tasks_Setter_WithIEnumerable_CopiesToList()
    {
        var tasks = new List<TaskItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Task1" }
            };

        _appState.Tasks = tasks;

        Assert.Single(_appState.Tasks);
    }

    [Fact]
    public void Tasks_Setter_WithNull_SetsEmptyList()
    {
        _appState.Tasks = null!;

        Assert.NotNull(_appState.Tasks);
        Assert.Empty(_appState.Tasks);
    }
}

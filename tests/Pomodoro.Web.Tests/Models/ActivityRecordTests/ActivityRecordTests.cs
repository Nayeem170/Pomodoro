using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

/// <summary>
/// Tests for ActivityRecord model
/// </summary>
public class ActivityRecordTests
{
    public class Constructor : ActivityRecordTests
    {
        [Fact]
        public void ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var record = new ActivityRecord();

            // Assert
            Assert.NotEqual(Guid.Empty, record.Id);
            Assert.Equal(SessionType.Pomodoro, record.Type); // default enum value
            Assert.Null(record.TaskName);
            Assert.Null(record.TaskId);
            Assert.Equal(default, record.CompletedAt);
            Assert.Equal(0, record.DurationMinutes);
            Assert.False(record.WasCompleted);
        }

        [Fact]
        public void ShouldGenerateUniqueIds()
        {
            // Arrange & Act
            var record1 = new ActivityRecord();
            var record2 = new ActivityRecord();

            // Assert
            Assert.NotEqual(record1.Id, record2.Id);
        }
    }

    public class DisplayText : ActivityRecordTests
    {
        [Fact]
        public void ShouldReturnTaskName_WhenPomodoroWithTask()
        {
            // Arrange
            var record = new ActivityRecord
            {
                Type = SessionType.Pomodoro,
                TaskName = "Test Task"
            };

            // Act
            var displayText = record.DisplayText;

            // Assert
            Assert.Equal("Test Task", displayText);
        }

        [Fact]
        public void ShouldReturnFocusTimeActivity_WhenPomodoroWithoutTask()
        {
            // Arrange
            var record = new ActivityRecord
            {
                Type = SessionType.Pomodoro,
                TaskName = null
            };

            // Act
            var displayText = record.DisplayText;

            // Assert
            Assert.Equal(Constants.SessionTypes.FocusTimeActivity, displayText);
        }

        [Fact]
        public void ShouldReturnShortBreakActivity_WhenShortBreakType()
        {
            // Arrange
            var record = new ActivityRecord
            {
                Type = SessionType.ShortBreak
            };

            // Act
            var displayText = record.DisplayText;

            // Assert
            Assert.Equal(Constants.SessionTypes.ShortBreakActivity, displayText);
        }

        [Fact]
        public void ShouldReturnLongBreakActivity_WhenLongBreakType()
        {
            // Arrange
            var record = new ActivityRecord
            {
                Type = SessionType.LongBreak
            };

            // Act
            var displayText = record.DisplayText;

            // Assert
            Assert.Equal(Constants.SessionTypes.LongBreakActivity, displayText);
        }
    }

    public class Icon : ActivityRecordTests
    {
        [Fact]
        public void ShouldReturnPomodoroEmoji_WhenPomodoroType()
        {
            // Arrange
            var record = new ActivityRecord
            {
                Type = SessionType.Pomodoro
            };

            // Act
            var icon = record.Icon;

            // Assert
            Assert.Equal(Constants.SessionTypes.PomodoroEmoji, icon);
        }

        [Fact]
        public void ShouldReturnShortBreakEmoji_WhenShortBreakType()
        {
            // Arrange
            var record = new ActivityRecord
            {
                Type = SessionType.ShortBreak
            };

            // Act
            var icon = record.Icon;

            // Assert
            Assert.Equal(Constants.SessionTypes.ShortBreakEmoji, icon);
        }

        [Fact]
        public void ShouldReturnLongBreakEmoji_WhenLongBreakType()
        {
            // Arrange
            var record = new ActivityRecord
            {
                Type = SessionType.LongBreak
            };

            // Act
            var icon = record.Icon;

            // Assert
            Assert.Equal(Constants.SessionTypes.LongBreakEmoji, icon);
        }
    }

    public class Properties : ActivityRecordTests
    {
        [Fact]
        public void ShouldSetAndGetTaskId()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Act
            var record = new ActivityRecord { TaskId = taskId };

            // Assert
            Assert.Equal(taskId, record.TaskId);
        }

        [Fact]
        public void ShouldSetAndGetDurationMinutes()
        {
            // Arrange & Act
            var record = new ActivityRecord { DurationMinutes = 25 };

            // Assert
            Assert.Equal(25, record.DurationMinutes);
        }

        [Fact]
        public void ShouldSetAndGetWasCompleted()
        {
            // Arrange & Act
            var record = new ActivityRecord { WasCompleted = true };

            // Assert
            Assert.True(record.WasCompleted);
        }

        [Fact]
        public void ShouldSetAndGetCompletedAt()
        {
            // Arrange
            var completedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            // Act
            var record = new ActivityRecord { CompletedAt = completedAt };

            // Assert
            Assert.Equal(completedAt, record.CompletedAt);
        }
    }
}

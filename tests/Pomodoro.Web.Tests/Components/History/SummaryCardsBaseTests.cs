using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Web.Components.History;
using Pomodoro.Web.Services.Formatters;
using Xunit;

#pragma warning disable BL0005 // Component parameters are set directly for testing purposes

namespace Pomodoro.Web.Tests.History;

/// <summary>
/// Testable wrapper for SummaryCardsBase to expose protected members
/// </summary>
public class TestableSummaryCardsBase : SummaryCardsBase
{
    public new SummaryCardsFormatter? Formatter
    {
        get => base.Formatter;
        set => base.Formatter = value!;
    }

    public new string FormatTime(int minutes) => base.FormatTime(minutes);

    public new string FormatCount(int count) => base.FormatCount(count);
}

/// <summary>
/// Unit tests for SummaryCardsBase class
/// </summary>
public class SummaryCardsBaseTests
{
    public class FormatTimeTests : TestContext
    {
        private readonly SummaryCardsFormatter _formatter;

        public FormatTimeTests()
        {
            var timeFormatter = new TimeFormatter();
            _formatter = new SummaryCardsFormatter(timeFormatter);
            Services.AddSingleton(_formatter);
        }

        [Fact]
        public void FormatTime_WithZeroMinutes()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int minutes = 0;

            // Act
            var result = baseComponent.FormatTime(minutes);

            // Assert
            Assert.Equal("0m", result);
        }

        [Fact]
        public void FormatTime_WithMinutesOnly()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int minutes = 45;

            // Act
            var result = baseComponent.FormatTime(minutes);

            // Assert
            Assert.Equal("45m", result);
        }

        [Fact]
        public void FormatTime_WithHoursAndMinutes()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int minutes = 90;

            // Act
            var result = baseComponent.FormatTime(minutes);

            // Assert
            Assert.Equal("1h 30m", result);
        }

        [Fact]
        public void FormatTime_WithComplexTime()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int minutes = 125;

            // Act
            var result = baseComponent.FormatTime(minutes);

            // Assert
            Assert.Equal("2h 5m", result);
        }

        [Fact]
        public void FormatTime_WithLargeMinutes()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int minutes = 480;

            // Act
            var result = baseComponent.FormatTime(minutes);

            // Assert
            Assert.Equal("8h 0m", result);
        }
    }

    public class FormatCountTests : TestContext
    {
        private readonly SummaryCardsFormatter _formatter;

        public FormatCountTests()
        {
            var timeFormatter = new TimeFormatter();
            _formatter = new SummaryCardsFormatter(timeFormatter);
            Services.AddSingleton(_formatter);
        }

        [Fact]
        public void FormatCount_WithZeroCount()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int count = 0;

            // Act
            var result = baseComponent.FormatCount(count);

            // Assert
            Assert.Equal("0 sessions", result);
        }

        [Fact]
        public void FormatCount_WithSmallCount()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int count = 10;

            // Act
            var result = baseComponent.FormatCount(count);

            // Assert
            Assert.Equal("10 sessions", result);
        }

        [Fact]
        public void FormatCount_WithLargeCount()
        {
            // Arrange
            var baseComponent = new TestableSummaryCardsBase
            {
                Formatter = _formatter
            };
            const int count = 100;

            // Act
            var result = baseComponent.FormatCount(count);

            // Assert
            Assert.Equal("100 sessions", result);
        }
    }

    public class ParameterTests
    {
        [Fact]
        public void PomodoroCount_DefaultValue()
        {
            // Arrange & Act
            var baseComponent = new TestableSummaryCardsBase();

            // Assert
            Assert.Equal(0, baseComponent.PomodoroCount);
        }

        [Fact]
        public void FocusMinutes_DefaultValue()
        {
            // Arrange & Act
            var baseComponent = new TestableSummaryCardsBase();

            // Assert
            Assert.Equal(0, baseComponent.FocusMinutes);
        }

        [Fact]
        public void TasksWorkedOn_DefaultValue()
        {
            // Arrange & Act
            var baseComponent = new TestableSummaryCardsBase();

            // Assert
            Assert.Equal(0, baseComponent.TasksWorkedOn);
        }

        [Fact]
        public void CanSetParameters()
        {
            // Arrange & Act
            var baseComponent = new TestableSummaryCardsBase
            {
                PomodoroCount = 5,
                FocusMinutes = 150,
                TasksWorkedOn = 3
            };

            // Assert
            Assert.Equal(5, baseComponent.PomodoroCount);
            Assert.Equal(150, baseComponent.FocusMinutes);
            Assert.Equal(3, baseComponent.TasksWorkedOn);
        }

        [Fact]
        public void CanSetNegativeParameters()
        {
            // Arrange & Act
            var baseComponent = new TestableSummaryCardsBase
            {
                PomodoroCount = -1,
                FocusMinutes = -10,
                TasksWorkedOn = -5
            };

            // Assert
            Assert.Equal(-1, baseComponent.PomodoroCount);
            Assert.Equal(-10, baseComponent.FocusMinutes);
            Assert.Equal(-5, baseComponent.TasksWorkedOn);
        }
    }

    /// <summary>
    /// Tests that exercise Formatter property getter and setter directly
    /// to improve branch coverage for line 28
    /// </summary>
    public class FormatterPropertyTests : TestContext
    {
        public FormatterPropertyTests()
        {
            Services.AddSingleton<TimeFormatter>();
            Services.AddSingleton<SummaryCardsFormatter>();
            Services.AddSingleton<StatCardFormatter>();
        }

        [Fact]
        public void Formatter_PropertyGetter_ReturnsInjectedValue()
        {
            // Arrange
            var timeFormatter = new TimeFormatter();
            var summaryCardsFormatter = new SummaryCardsFormatter(timeFormatter);
            Services.AddSingleton(summaryCardsFormatter);
            
            var cut = new TestableSummaryCardsBase();
            
            // Act - Set the Formatter property through DI injection
            cut.Formatter = summaryCardsFormatter;
            
            // Act - Access the Formatter property getter through the testable wrapper
            var formatterValue = cut.Formatter;
            
            // Assert
            Assert.NotNull(formatterValue);
            Assert.IsType<SummaryCardsFormatter>(formatterValue);
        }

        [Fact]
        public void Formatter_PropertySetter_CanSetNewValue()
        {
            // Arrange
            var timeFormatter = new TimeFormatter();
            var newFormatter = new SummaryCardsFormatter(timeFormatter);
            
            var cut = new TestableSummaryCardsBase();
            
            // Act - Set the Formatter property through the testable wrapper (exercises setter)
            cut.Formatter = newFormatter;
            
            // Assert - Verify the setter worked by getting the value back
            var updatedValue = cut.Formatter;
            Assert.NotNull(updatedValue);
            Assert.Same(newFormatter, updatedValue);
        }
    }

    /// <summary>
    /// Tests that exercise SummaryCardsBase through the SummaryCards component
    /// to ensure proper coverage of [Parameter] and [Inject] attributes
    /// </summary>
    public class ComponentRenderingTests : TestContext
    {
        public ComponentRenderingTests()
        {
            Services.AddSingleton<TimeFormatter>();
            Services.AddSingleton<SummaryCardsFormatter>();
            Services.AddSingleton<StatCardFormatter>();
        }

        [Fact]
        public void SummaryCards_PomodoroCountParameter_IsRendered()
        {
            // Arrange & Act - This exercises the [Parameter] attribute on PomodoroCount (line14)
            var cut = RenderComponent<SummaryCards>(parameters => parameters
                .Add(p => p.PomodoroCount, 5));

            // Assert
            Assert.Contains("5", cut.Markup);
        }

        [Fact]
        public void SummaryCards_FocusMinutesParameter_IsRendered()
        {
            // Arrange & Act - This exercises the [Parameter] attribute on FocusMinutes (line17)
            var cut = RenderComponent<SummaryCards>(parameters => parameters
                .Add(p => p.FocusMinutes, 90));

            // Assert
            Assert.Contains("1h 30m", cut.Markup);
        }

        [Fact]
        public void SummaryCards_TasksWorkedOnParameter_IsRendered()
        {
            // Arrange & Act - This exercises the [Parameter] attribute on TasksWorkedOn (line20)
            var cut = RenderComponent<SummaryCards>(parameters => parameters
                .Add(p => p.TasksWorkedOn, 7));

            // Assert
            Assert.Contains("7", cut.Markup);
        }

        [Fact]
        public void SummaryCards_InjectFormatter_IsUsed()
        {
            // Arrange & Act - This exercises the [Inject] attribute on Formatter (line27)
            var cut = RenderComponent<SummaryCards>(parameters => parameters
                .Add(p => p.PomodoroCount, 10)
                .Add(p => p.FocusMinutes, 120)
                .Add(p => p.TasksWorkedOn, 3));

            // Assert - Verify the injected formatter is being used
            Assert.Contains("2h", cut.Markup); //120 minutes = 2h
            Assert.Contains("10", cut.Markup);
            Assert.Contains("3", cut.Markup);
        }

        [Fact]
        public void SummaryCards_AllParametersZero_IsRendered()
        {
            // Arrange & Act - Exercise all parameters with zero values
            var cut = RenderComponent<SummaryCards>(parameters => parameters
                .Add(p => p.PomodoroCount, 0)
                .Add(p => p.FocusMinutes, 0)
                .Add(p => p.TasksWorkedOn, 0));

            // Assert
            Assert.NotNull(cut.Markup);
            Assert.Contains("0m", cut.Markup);
        }

        [Fact]
        public void SummaryCards_AllParametersWithValues_IsRendered()
        {
            // Arrange & Act - Exercise all parameters together
            var cut = RenderComponent<SummaryCards>(parameters => parameters
                .Add(p => p.PomodoroCount, 25)
                .Add(p => p.FocusMinutes, 625) // 10h 25m
                .Add(p => p.TasksWorkedOn, 12));

            // Assert
            Assert.Contains("10h", cut.Markup);
            Assert.Contains("25", cut.Markup);
            Assert.Contains("12", cut.Markup);
        }
    }
}

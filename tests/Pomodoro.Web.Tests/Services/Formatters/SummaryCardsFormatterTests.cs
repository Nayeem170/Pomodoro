using Xunit;
using Pomodoro.Web.Services.Formatters;

namespace Pomodoro.Web.Tests.Services.Formatters;

/// <summary>
/// Tests for SummaryCardsFormatter service
/// </summary>
public class SummaryCardsFormatterTests
{
    private readonly SummaryCardsFormatter _formatter;

    public SummaryCardsFormatterTests()
    {
        _formatter = new SummaryCardsFormatter(new TimeFormatter());
    }

    public class FormatTime : SummaryCardsFormatterTests
    {
        [Fact]
        public void FormatTime_ZeroMinutes_ReturnsZero()
        {
            // Act
            var result = _formatter.FormatTime(0);

            // Assert
            Assert.Equal("0m", result);
        }

        [Fact]
        public void FormatTime_LessThanOneHour_ReturnsMinutesOnly()
        {
            // Act
            var result = _formatter.FormatTime(30);

            // Assert
            Assert.Equal("30m", result);
        }

        [Fact]
        public void FormatTime_ExactlyOneHour_ReturnsHoursAndZeroMinutes()
        {
            // Act
            var result = _formatter.FormatTime(60);

            // Assert
            Assert.Equal("1h 0m", result);
        }

        [Fact]
        public void FormatTime_MoreThanOneHour_ReturnsHoursAndMinutes()
        {
            // Act
            var result = _formatter.FormatTime(90);

            // Assert
            Assert.Equal("1h 30m", result);
        }

        [Fact]
        public void FormatTime_MultipleHours_ReturnsCorrectFormat()
        {
            // Act
            var result = _formatter.FormatTime(150);

            // Assert
            Assert.Equal("2h 30m", result);
        }

        [Theory]
        [InlineData(0, "0m")]
        [InlineData(1, "1m")]
        [InlineData(59, "59m")]
        [InlineData(60, "1h 0m")]
        [InlineData(61, "1h 1m")]
        [InlineData(120, "2h 0m")]
        [InlineData(121, "2h 1m")]
        [InlineData(150, "2h 30m")]
        [InlineData(180, "3h 0m")]
        [InlineData(300, "5h 0m")]
        public void FormatTime_VariousValues_ReturnsCorrectFormat(int minutes, string expected)
        {
            // Act
            var result = _formatter.FormatTime(minutes);

            // Assert
            Assert.Equal(expected, result);
        }
    }

    public class FormatCount : SummaryCardsFormatterTests
    {
        [Fact]
        public void FormatCount_Zero_ReturnsZeroSessions()
        {
            // Act
            var result = _formatter.FormatCount(0);

            // Assert
            Assert.Equal("0 sessions", result);
        }

        [Fact]
        public void FormatCount_One_ReturnsOneSession()
        {
            // Act
            var result = _formatter.FormatCount(1);

            // Assert
            Assert.Equal("1 sessions", result);
        }

        [Fact]
        public void FormatCount_Multiple_ReturnsCorrectFormat()
        {
            // Act
            var result = _formatter.FormatCount(5);

            // Assert
            Assert.Equal("5 sessions", result);
        }

        [Theory]
        [InlineData(0, "0 sessions")]
        [InlineData(1, "1 sessions")]
        [InlineData(10, "10 sessions")]
        [InlineData(100, "100 sessions")]
        public void FormatCount_VariousValues_ReturnsCorrectFormat(int count, string expected)
        {
            // Act
            var result = _formatter.FormatCount(count);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

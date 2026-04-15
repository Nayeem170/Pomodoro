using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Unit tests for LocalDateTimeService
/// </summary>
[Trait("Category", "Service")]
public class LocalDateTimeServiceTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidJSRuntime_InitializesSuccessfully()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>().Object;

        // Act
        var service = new LocalDateTimeService(jsRuntime);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullJSRuntime_InitializesSuccessfully()
    {
        // Arrange
        IJSRuntime jsRuntime = null!;

        // Act
        var service = new LocalDateTimeService(jsRuntime);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void ImplementsILocalDateTimeService()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>().Object;

        // Act
        var service = new LocalDateTimeService(jsRuntime);

        // Assert
        service.Should().BeAssignableTo<ILocalDateTimeService>();
    }

    #endregion

    #region GetLocalDateAsync Tests

    [Fact]
    public async Task GetLocalDateAsync_FirstCall_InvokesJSInteropAndReturnsDate()
    {
        // Arrange
        var expectedDate = DateTime.Now.Date;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(expectedDate));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetLocalDateAsync();

        // Assert
        result.Should().Be(expectedDate);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetLocalDateAsync_SecondCall_ReturnsCachedValue()
    {
        // Arrange
        var firstDate = DateTime.Now.Date;
        var secondDate = DateTime.Now.Date;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(firstDate));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result1 = await service.GetLocalDateAsync();
        var result2 = await service.GetLocalDateAsync();

        // Assert
        result1.Should().Be(firstDate);
        result2.Should().Be(firstDate);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetLocalDateAsync_WhenJSInteropFails_ReturnsServerDate()
    {
        // Arrange
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()))
            .Throws(new Exception("JS interop failed"));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetLocalDateAsync();

        // Assert
        result.Should().BeCloseTo(DateTime.Now.Date, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetLocalDateAsync_AfterCacheClear_InvokesJSInteropAgain()
    {
        // Arrange
        var firstDate = DateTime.Now.Date;
        var secondDate = DateTime.Now.Date.AddDays(1);
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .SetupSequence(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(firstDate))
            .Returns(new ValueTask<DateTime>(secondDate));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result1 = await service.GetLocalDateAsync();
        service.ClearCache();
        var result2 = await service.GetLocalDateAsync();

        // Assert
        result1.Should().Be(firstDate);
        result2.Should().Be(secondDate);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()), Times.Exactly(2));
    }

    #endregion

    #region GetLocalDateTimeOffsetAsync Tests

    [Fact]
    public async Task GetLocalDateTimeOffsetAsync_FirstCall_InvokesJSInteropAndReturnsOffset()
    {
        // Arrange
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var expectedOffset = 300;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(expectedDateTime));
        mockRuntime
            .Setup(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(expectedOffset));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetLocalDateTimeOffsetAsync();

        // Assert
        result.DateTime.Should().Be(expectedDateTime);
        result.Offset.Should().Be(TimeSpan.FromMinutes(-expectedOffset));
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()), Times.Once);
        mockRuntime.Verify(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetLocalDateTimeOffsetAsync_SecondCall_ReturnsCachedValue()
    {
        // Arrange
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var expectedOffset = 300;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(expectedDateTime));
        mockRuntime
            .Setup(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(expectedOffset));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result1 = await service.GetLocalDateTimeOffsetAsync();
        var result2 = await service.GetLocalDateTimeOffsetAsync();

        // Assert
        result1.Should().Be(result2);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()), Times.Once);
        mockRuntime.Verify(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetLocalDateTimeOffsetAsync_WhenJSInteropFails_ReturnsServerDateTime()
    {
        // Arrange
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Throws(new Exception("JS interop failed"));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetLocalDateTimeOffsetAsync();

        // Assert
        result.Should().BeCloseTo(DateTimeOffset.Now, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetLocalDateTimeOffsetAsync_AfterCacheClear_InvokesJSInteropAgain()
    {
        // Arrange
        var firstDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var firstOffset = 300;
        var secondDateTime = new DateTime(2024, 1, 1, 13, 0, 0);
        var secondOffset = 240;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .SetupSequence(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(firstDateTime))
            .Returns(new ValueTask<DateTime>(secondDateTime));
        mockRuntime
            .SetupSequence(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(firstOffset))
            .Returns(new ValueTask<int>(secondOffset));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result1 = await service.GetLocalDateTimeOffsetAsync();
        service.ClearCache();
        var result2 = await service.GetLocalDateTimeOffsetAsync();

        // Assert
        result1.DateTime.Should().Be(firstDateTime);
        result2.DateTime.Should().Be(secondDateTime);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()), Times.Exactly(2));
        mockRuntime.Verify(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetLocalDateTimeOffsetAsync_WithNegativeOffset_CreatesCorrectOffset()
    {
        // Arrange
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var expectedOffset = -300;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(expectedDateTime));
        mockRuntime
            .Setup(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(expectedOffset));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetLocalDateTimeOffsetAsync();

        // Assert
        result.DateTime.Should().Be(expectedDateTime);
        result.Offset.Should().Be(TimeSpan.FromMinutes(300));
    }

    #endregion

    #region GetLocalDateTimeAsync Tests

    [Fact]
    public async Task GetLocalDateTimeAsync_InvokesJSInteropAndReturnsDateTime()
    {
        // Arrange
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 30, 0);
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(expectedDateTime));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetLocalDateTimeAsync();

        // Assert
        result.Should().Be(expectedDateTime);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetLocalDateTimeAsync_WhenJSInteropFails_ReturnsServerDateTime()
    {
        // Arrange
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Throws(new Exception("JS interop failed"));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetLocalDateTimeAsync();

        // Assert
        result.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetLocalDateTimeAsync_MultipleCalls_InvokesJSInteropEachTime()
    {
        // Arrange
        var firstDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var secondDateTime = new DateTime(2024, 1, 1, 12, 30, 0);
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .SetupSequence(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(firstDateTime))
            .Returns(new ValueTask<DateTime>(secondDateTime));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result1 = await service.GetLocalDateTimeAsync();
        var result2 = await service.GetLocalDateTimeAsync();

        // Assert
        result1.Should().Be(firstDateTime);
        result2.Should().Be(secondDateTime);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()), Times.Exactly(2));
    }

    #endregion

    #region GetTimezoneOffsetAsync Tests

    [Fact]
    public async Task GetTimezoneOffsetAsync_InvokesJSInteropAndReturnsOffset()
    {
        // Arrange
        var expectedOffset = 300;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(expectedOffset));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetTimezoneOffsetAsync();

        // Assert
        result.Should().Be(expectedOffset);
        mockRuntime.Verify(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetTimezoneOffsetAsync_WhenJSInteropFails_ReturnsServerOffset()
    {
        // Arrange
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Throws(new Exception("JS interop failed"));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result = await service.GetTimezoneOffsetAsync();

        // Assert
        var serverOffset = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
        result.Should().Be(serverOffset);
    }

    [Fact]
    public async Task GetTimezoneOffsetAsync_MultipleCalls_InvokesJSInteropEachTime()
    {
        // Arrange
        var firstOffset = 300;
        var secondOffset = 240;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .SetupSequence(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(firstOffset))
            .Returns(new ValueTask<int>(secondOffset));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var result1 = await service.GetTimezoneOffsetAsync();
        var result2 = await service.GetTimezoneOffsetAsync();

        // Assert
        result1.Should().Be(firstOffset);
        result2.Should().Be(secondOffset);
        mockRuntime.Verify(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()), Times.Exactly(2));
    }

    #endregion

    #region ClearCache Tests

    [Fact]
    public async Task ClearCache_SetsCachedValuesToNull()
    {
        // Arrange
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(DateTime.Now.Date));
        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act - Prime cache
        var _ = service.GetLocalDateAsync();
        service.ClearCache();

        // Act - Should call JS interop again after cache clear
        var __ = service.GetLocalDateAsync();

        // Assert
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()), Times.Exactly(2));
    }

    [Fact]
    public void ClearCache_MakesMultipleCacheClearsSafe()
    {
        // Arrange
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(DateTime.Now.Date));
        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        service.ClearCache();
        service.ClearCache();
        service.ClearCache();

        // Assert - Should not throw
        service.Should().NotBeNull();
    }

    #endregion

    #region Combined Method Tests

    [Fact]
    public async Task MultipleMethods_ShouldCacheIndependently()
    {
        // Arrange
        var expectedDate = DateTime.Now.Date;
        var expectedDateTime = new DateTime(2024, 1, 1, 12, 0, 0);
        var expectedOffset = 300;
        var mockRuntime = new Mock<IJSRuntime>();
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(expectedDate));
        mockRuntime
            .Setup(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()))
            .Returns(new ValueTask<DateTime>(expectedDateTime));
        mockRuntime
            .Setup(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()))
            .Returns(new ValueTask<int>(expectedOffset));

        var service = new LocalDateTimeService(mockRuntime.Object);

        // Act
        var date1 = await service.GetLocalDateAsync();
        var date2 = await service.GetLocalDateAsync();
        var offset1 = await service.GetLocalDateTimeOffsetAsync();
        var offset2 = await service.GetLocalDateTimeOffsetAsync();

        // Assert
        date1.Should().Be(date2);
        offset1.Should().Be(offset2);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDate", It.IsAny<object[]>()), Times.Once);
        mockRuntime.Verify(x => x.InvokeAsync<DateTime>("localDateTime.getLocalDateTime", It.IsAny<object[]>()), Times.Once);
        mockRuntime.Verify(x => x.InvokeAsync<int>("localDateTime.getTimezoneOffset", It.IsAny<object[]>()), Times.Once);
    }

    #endregion
}


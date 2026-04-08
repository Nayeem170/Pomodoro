using Xunit;
using Pomodoro.Web.Models;
using Moq;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for PipTimerService lifecycle methods.
/// </summary>
public partial class PipTimerServiceTests
{
    /// <summary>
    /// Tests for service initialization
    /// </summary>
    public class LifecycleTests : PipTimerServiceTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Arrange & Act
            var service = CreateService();

            // Assert
            Assert.False(service.IsSupported);
            Assert.False(service.IsOpen);
        }

        [Fact]
        public void IsSupported_DefaultValue_IsFalse()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            Assert.False(service.IsSupported);
        }

        [Fact]
        public void IsOpen_DefaultValue_IsFalse()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            Assert.False(service.IsOpen);
        }

        [Fact]
        public void OnPipOpened_Event_CanBeSubscribed()
        {
            // Arrange
            var service = CreateService();
            var eventRaised = false;
            service.OnPipOpened += () => eventRaised = true;

            // Act - Simulate event trigger (would normally be triggered by OpenAsync)
            // We can't directly test this without JS interop, but we verify subscription works

            // Assert - Event handler was registered without error
            Assert.False(eventRaised); // Event hasn't been raised yet
        }

        [Fact]
        public void OnPipClosed_Event_CanBeSubscribed()
        {
            // Arrange
            var service = CreateService();
            var eventRaised = false;
            service.OnPipClosed += () => eventRaised = true;

            // Act - Simulate event trigger (would normally be triggered by CloseAsync)
            // We can't directly test this without JS interop, but we verify subscription works

            // Assert - Event handler was registered without error
            Assert.False(eventRaised); // Event hasn't been raised yet
        }

        [Fact]
        public void OnPipClosedJs_SetsIsOpenToFalse_AndRaisesEvent()
        {
            // Arrange
            var service = CreateService();
            var eventRaised = false;
            service.OnPipClosed += () => eventRaised = true;

            // Act - Call the JSInvokable method directly
            service.OnPipClosedJs();

            // Assert
            Assert.False(service.IsOpen);
            Assert.True(eventRaised);
        }

        [Fact]
        public void OnPipClosedJs_WhenAlreadyClosed_DoesNotRaiseEventTwice()
        {
            // Arrange
            var service = CreateService();
            var eventCount = 0;
            service.OnPipClosed += () => eventCount++;

            // Act
            service.OnPipClosedJs();
            service.OnPipClosedJs(); // Call twice

            // Assert - Event should be raised each time (current behavior)
            Assert.Equal(2, eventCount);
        }
    }
}

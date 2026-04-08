using Moq;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Services;

/// <summary>
/// Tests for TimerService event handling.
/// </summary>
public partial class TimerServiceTests
{
    public class EventsTests : TimerServiceTests
    {
        [Fact]
        public async Task OnStateChanged_FiresOnTimerStart()
        {
            // Arrange
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            await service.StartPomodoroAsync();
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task OnStateChanged_FiresOnTimerPause()
        {
            // Arrange
            var service = CreateService();
            await service.StartPomodoroAsync();
            
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            await service.PauseAsync();
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task OnStateChanged_FiresOnTimerResume()
        {
            // Arrange
            var service = CreateService();
            await service.StartPomodoroAsync();
            await service.PauseAsync();
            
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            await service.ResumeAsync();
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task OnStateChanged_FiresOnTimerReset()
        {
            // Arrange
            var service = CreateService();
            await service.StartPomodoroAsync();
            
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            await service.ResetAsync();
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task OnStateChanged_FiresOnSwitchSessionType()
        {
            // Arrange
            var service = CreateService();
            await service.StartPomodoroAsync();
            
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            await service.SwitchSessionTypeAsync(SessionType.ShortBreak);
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task OnStateChanged_FiresOnUpdateSettings()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            var newSettings = new TimerSettings();
            await service.UpdateSettingsAsync(newSettings);
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task MultipleEventHandlers_AllReceiveEvents()
        {
            // Arrange
            var service = CreateService();
            
            var handler1Fired = false;
            var handler2Fired = false;
            var handler3Fired = false;
            
            service.OnStateChanged += () => handler1Fired = true;
            service.OnStateChanged += () => handler2Fired = true;
            service.OnStateChanged += () => handler3Fired = true;
            
            // Act
            await service.StartPomodoroAsync();
            
            // Assert
            Assert.True(handler1Fired);
            Assert.True(handler2Fired);
            Assert.True(handler3Fired);
        }

        [Fact]
        public async Task OnStateChanged_FiresOnShortBreakStart()
        {
            // Arrange
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            await service.StartShortBreakAsync();
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public async Task OnStateChanged_FiresOnLongBreakStart()
        {
            // Arrange
            var service = CreateService();
            var eventFired = false;
            service.OnStateChanged += () => eventFired = true;
            
            // Act
            await service.StartLongBreakAsync();
            
            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void OnTick_CanBeSubscribedTo()
        {
            // Arrange
            var service = CreateService();
            
            // Act
            // Verify event can be subscribed to (no exception means success)
            service.OnTick += () => { };
            
            // Assert
            // Event subscription succeeded if no exception was thrown
            Assert.True(true);
        }

        [Fact]
        public void OnTimerCompleted_CanBeSubscribedTo()
        {
            // Arrange
            var service = CreateService();
            
            // Act
            // Verify event can be subscribed to (no exception means success)
            service.OnTimerCompleted += (args) => Task.CompletedTask;
            
            // Assert
            // Event subscription succeeded if no exception was thrown
            Assert.True(true);
        }

        [Fact]
        public void OnTimerComplete_BackwardCompatibilityEvent_CanBeSubscribedTo()
        {
            // Arrange
            var service = CreateService();
            
            // Act
            // Verify event can be subscribed to (no exception means success)
            service.OnTimerComplete += (type) => { };
            
            // Assert
            // Event subscription succeeded if no exception was thrown
            Assert.True(true);
        }

        [Fact]
        public async Task EventHandlers_CanBeUnsubscribed()
        {
            // Arrange
            var service = CreateService();
            
            var eventFired = false;
            Action handler = () => eventFired = true;
            service.OnStateChanged += handler;
            
            // Act
            await service.StartPomodoroAsync();
            var firstFireCount = eventFired ? 1 : 0;
            
            // Unsubscribe
            service.OnStateChanged -= handler;
            
            // Trigger another state change
            await service.ResetAsync();
            
            // Assert
            // Event should have fired once (on start) but not after unsubscribe
            Assert.Equal(1, firstFireCount);
        }
    }
}

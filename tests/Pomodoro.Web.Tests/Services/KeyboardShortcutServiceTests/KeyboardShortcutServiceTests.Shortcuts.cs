using Moq;
using Xunit;

namespace Pomodoro.Web.Tests.Services.KeyboardShortcutServiceTests;

[Trait("Category", "Service")]
public partial class KeyboardShortcutServiceTests
{
    public class RegisterShortcutTests : KeyboardShortcutServiceTests
    {
        [Fact]
        public void RegisterShortcut_NormalizesKeyToLowercase()
        {
            // Arrange
            var service = CreateService();
            var actionCalled = false;

            // Act
            service.RegisterShortcut("A", () => actionCalled = true, "Test shortcut");

            // Assert - verify by checking HandleShortcut works with lowercase
            service.HandleShortcut("a");
            Assert.True(actionCalled);
        }

        [Fact]
        public void RegisterShortcut_WithDescription_StoresDescription()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.RegisterShortcut("s", () => { }, "Save action");

            // Assert
            var shortcuts = service.GetRegisteredShortcuts();
            Assert.Contains("s", shortcuts.Keys);
            Assert.Equal("Save action", shortcuts["s"]);
        }

        [Fact]
        public void RegisterShortcut_WithoutDescription_DoesNotStoreDescription()
        {
            // Arrange
            var service = CreateService();

            // Act
            service.RegisterShortcut("x", () => { });

            // Assert
            var shortcuts = service.GetRegisteredShortcuts();
            Assert.DoesNotContain("x", shortcuts.Keys);
        }

        [Fact]
        public void RegisterShortcut_OverwritesExisting()
        {
            // Arrange
            var service = CreateService();
            var firstCalled = false;
            var secondCalled = false;

            // Act
            service.RegisterShortcut("a", () => firstCalled = true);
            service.RegisterShortcut("A", () => secondCalled = true); // Same key normalized

            // Assert
            service.HandleShortcut("a");
            Assert.False(firstCalled);
            Assert.True(secondCalled);
        }
    }

    public class UnregisterShortcutTests : KeyboardShortcutServiceTests
    {
        [Fact]
        public void UnregisterShortcut_RemovesShortcut()
        {
            // Arrange
            var service = CreateService();
            var actionCalled = false;
            service.RegisterShortcut("d", () => actionCalled = true, "Delete");

            // Act
            service.UnregisterShortcut("d");

            // Assert
            service.HandleShortcut("d");
            Assert.False(actionCalled);
        }

        [Fact]
        public void UnregisterShortcut_NormalizesKey()
        {
            // Arrange
            var service = CreateService();
            var actionCalled = false;
            service.RegisterShortcut("d", () => actionCalled = true, "Delete");

            // Act
            service.UnregisterShortcut("D"); // Uppercase

            // Assert
            service.HandleShortcut("d");
            Assert.False(actionCalled);
        }

        [Fact]
        public void UnregisterShortcut_RemovesDescription()
        {
            // Arrange
            var service = CreateService();
            service.RegisterShortcut("d", () => { }, "Delete");

            // Act
            service.UnregisterShortcut("d");

            // Assert
            var shortcuts = service.GetRegisteredShortcuts();
            Assert.DoesNotContain("d", shortcuts.Keys);
        }

        [Fact]
        public void UnregisterShortcut_NonExistentKey_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert - should not throw
            service.UnregisterShortcut("nonexistent");
        }
    }

    public class GetRegisteredShortcutsTests : KeyboardShortcutServiceTests
    {
        [Fact]
        public void GetRegisteredShortcuts_ReturnsCopy()
        {
            // Arrange
            var service = CreateService();
            service.RegisterShortcut("a", () => { }, "Action A");

            // Act
            var shortcuts1 = service.GetRegisteredShortcuts();
            service.RegisterShortcut("b", () => { }, "Action B");
            var shortcuts2 = service.GetRegisteredShortcuts();

            // Assert
            Assert.Single(shortcuts1);
            Assert.Equal(2, shortcuts2.Count);
        }

        [Fact]
        public void GetRegisteredShortcuts_WhenEmpty_ReturnsEmptyDictionary()
        {
            // Arrange
            var service = CreateService();

            // Act
            var shortcuts = service.GetRegisteredShortcuts();

            // Assert
            Assert.Empty(shortcuts);
        }
    }

    public class HandleShortcutTests : KeyboardShortcutServiceTests
    {
        [Fact]
        public void HandleShortcut_ExecutesRegisteredAction()
        {
            // Arrange
            var service = CreateService();
            var actionCalled = false;
            service.RegisterShortcut("space", () => actionCalled = true);

            // Act
            service.HandleShortcut("space");

            // Assert
            Assert.True(actionCalled);
        }

        [Fact]
        public void HandleShortcut_NormalizesKey()
        {
            // Arrange
            var service = CreateService();
            var actionCalled = false;
            service.RegisterShortcut("SPACE", () => actionCalled = true);

            // Act
            service.HandleShortcut("space");

            // Assert
            Assert.True(actionCalled);
        }

        [Fact]
        public void HandleShortcut_UnregisteredKey_DoesNothing()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert - should not throw
            service.HandleShortcut("unknown");
        }

        [Fact]
        public void HandleShortcut_ActionThrowsException_DoesNotPropagate()
        {
            // Arrange
            var service = CreateService();
            service.RegisterShortcut("crash", () => throw new InvalidOperationException("Test exception"));

            // Act & Assert - should not throw
            service.HandleShortcut("crash");
        }
    }

    public class InitializeAsyncTests : KeyboardShortcutServiceTests
    {
        [Fact]
        public async Task InitializeAsync_CreatesDotNetObjectReferenceAndCallsJS()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.InitializeAsync();

            // Assert - verify JS interop was called
            _mockJsRuntime.Verify(js => js.InvokeAsync<object>(
                "keyboardShortcuts.initialize",
                It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_CanBeCalledMultipleTimes()
        {
            // Arrange
            var service = CreateService();

            // Act - call twice
            await service.InitializeAsync();
            await service.InitializeAsync();

            // Assert - verify JS interop was called twice
            _mockJsRuntime.Verify(js => js.InvokeAsync<object>(
                "keyboardShortcuts.initialize",
                It.IsAny<object[]>()),
                Times.Exactly(2));
        }
    }

    public class DisposeAsyncTests : KeyboardShortcutServiceTests
    {
        [Fact]
        public async Task DisposeAsyncWithoutInitialize_DoesNotCallJSDispose()
        {
            // Arrange
            var service = CreateService();
            // Don't call InitializeAsync, so _dotNetRef is null

            // Act
            await service.DisposeAsync();

            // Assert - JS dispose should not be called since _dotNetRef is null
            _mockJsRuntime.Verify(js => js.InvokeAsync<object>(
                "keyboardShortcuts.dispose",
                It.IsAny<object[]>()),
                Times.Never);
        }

        [Fact]
        public async Task DisposeAsync_AfterInitialize_CallsJSDispose()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();

            // Act
            await service.DisposeAsync();

            // Assert - JS dispose should be called
            _mockJsRuntime.Verify(js => js.InvokeAsync<object>(
                "keyboardShortcuts.dispose",
                It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_HandlesJSException()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();
            _mockJsRuntime.Setup(js => js.InvokeAsync<object>(
                    "keyboardShortcuts.dispose",
                    It.IsAny<object[]>()))
                .ThrowsAsync(new InvalidOperationException("JS error"));

            // Act & Assert - should not throw
            await service.DisposeAsync();
        }

        [Fact]
        public async Task DisposeAsync_CanBeCalledMultipleTimes()
        {
            // Arrange
            var service = CreateService();
            await service.InitializeAsync();

            // Act - dispose twice
            await service.DisposeAsync();
            await service.DisposeAsync();

            // Assert - JS dispose should only be called once (second time _dotNetRef is null)
            _mockJsRuntime.Verify(js => js.InvokeAsync<object>(
                "keyboardShortcuts.dispose",
                It.IsAny<object[]>()),
                Times.Once);
        }
    }
}


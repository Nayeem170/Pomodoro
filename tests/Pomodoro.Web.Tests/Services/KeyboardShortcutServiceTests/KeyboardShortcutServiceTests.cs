using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Pomodoro.Web.Services;

namespace Pomodoro.Web.Tests.Services.KeyboardShortcutServiceTests;

using Xunit;
[Trait("Category", "Service")]
public partial class KeyboardShortcutServiceTests
{
    protected readonly Mock<IJSRuntime> _mockJsRuntime;
    protected readonly Mock<ILogger<KeyboardShortcutService>> _mockLogger;

    public KeyboardShortcutServiceTests()
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockLogger = new Mock<ILogger<KeyboardShortcutService>>();
    }

    protected KeyboardShortcutService CreateService()
    {
        return new KeyboardShortcutService(_mockJsRuntime.Object, _mockLogger.Object);
    }
}


using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pomodoro.Web.Components.Settings;
using Pomodoro.Web.Services;
using Xunit;

namespace Pomodoro.Web.Tests.Components.Settings;

[Trait("Category", "Component")]
public class CloudSyncSettingsCoverageTests : TestContext
{
    private readonly Mock<ICloudSyncService> _cloudSyncServiceMock;
    private readonly Mock<ILogger<CloudSyncSettings>> _loggerMock;

    public CloudSyncSettingsCoverageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _cloudSyncServiceMock = new Mock<ICloudSyncService>();
        _loggerMock = new Mock<ILogger<CloudSyncSettings>>();
        Services.AddSingleton(_cloudSyncServiceMock.Object);
        Services.AddSingleton(_loggerMock.Object);
    }

    [Fact]
    public void Connect_UsesClientId_WhenAvailable()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns("custom-client-id");
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(true);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button.sec-btn").Click();

        _cloudSyncServiceMock.Verify(x => x.ConnectAsync("custom-client-id"), Times.Once);
    }

    [Fact]
    public void Connect_HandlesException()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Connection failed"));

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.InitFailed));
    }

    [Fact]
    public void Sync_UpToDate_ShowsCorrectMessage()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync()).ReturnsAsync(SyncResult.UpToDate());

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.AlreadyUpToDate));
    }

    [Fact]
    public void Sync_ReconnectRequired_ReconnectsAndSyncs()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.SetupSequence(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Failed(Constants.SyncMessages.ReconnectRequired))
            .ReturnsAsync(SyncResult.Pushed());
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(true);

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.SyncPushSuccess));
        _cloudSyncServiceMock.Verify(x => x.ConnectAsync(It.IsAny<string>()), Times.Once);
        _cloudSyncServiceMock.Verify(x => x.SyncNowAsync(), Times.Exactly(2));
    }

    [Fact]
    public void Sync_ReconnectRequired_ReconnectFails_ShowsAuthFailed()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Failed(Constants.SyncMessages.ReconnectRequired));
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(false);

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.AuthFailed));
    }

    [Fact]
    public void Sync_ReconnectRequired_SecondSyncFails_ShowsErrorMessage()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.SetupSequence(x => x.SyncNowAsync())
            .ReturnsAsync(SyncResult.Failed(Constants.SyncMessages.ReconnectRequired))
            .ReturnsAsync(SyncResult.Failed("Second sync failed"));
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(true);

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be("Second sync failed"));
    }

    [Fact]
    public void Sync_HandlesException()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync())
            .ThrowsAsync(new Exception("Sync error"));

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.SyncFailed));
    }

    [Fact]
    public void Disconnect_HandlesException()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.DisconnectAsync())
            .ThrowsAsync(new Exception("Disconnect error"));

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.danger-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.SyncFailed));
    }

    [Fact]
    public void OnSyncStatusChanged_InvokesStateHasChanged()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);

        var cut = RenderComponent<CloudSyncSettings>();
        var before = cut.RenderCount;

        _cloudSyncServiceMock.Raise(x => x.OnSyncStatusChanged += null);

        cut.WaitForAssertion(() => cut.RenderCount.Should().BeGreaterThan(before));
    }

    [Fact]
    public void Dispose_UnsubscribesFromSyncStatusChanged()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Instance.Dispose();

        var ex = Record.Exception(() =>
            _cloudSyncServiceMock.Raise(x => x.OnSyncStatusChanged += null));
        Assert.Null(ex);
    }
}

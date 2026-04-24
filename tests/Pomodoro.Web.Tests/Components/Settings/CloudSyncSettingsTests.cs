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
public class CloudSyncSettingsTests : TestContext
{
    private readonly Mock<ICloudSyncService> _cloudSyncServiceMock;
    private readonly Mock<ILogger<CloudSyncSettings>> _loggerMock;

    public CloudSyncSettingsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _cloudSyncServiceMock = new Mock<ICloudSyncService>();
        _loggerMock = new Mock<ILogger<CloudSyncSettings>>();
        Services.AddSingleton(_cloudSyncServiceMock.Object);
        Services.AddSingleton(_loggerMock.Object);
    }

    [Fact]
    public void Render_ShowsConnectButton_WhenDisconnected()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button").TextContent.Should().Be("Connect");
        cut.FindAll("button").Should().HaveCount(1);
    }

    [Fact]
    public void Render_ShowsSyncAndDisconnect_WhenConnected()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);

        var cut = RenderComponent<CloudSyncSettings>();

        var buttons = cut.FindAll("button");
        buttons.Should().HaveCount(2);
        buttons[0].TextContent.Should().Be("Sync");
        buttons[1].TextContent.Should().Be("Disconnect");
    }

    [Fact]
    public void Render_ShowsLastSyncedTime()
    {
        var syncTime = new DateTime(2026, 4, 24, 10, 30, 0, DateTimeKind.Utc);
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns(syncTime);

        var cut = RenderComponent<CloudSyncSettings>();

        var markup = cut.Markup;
        markup.Should().Contain("Last synced");
        markup.Should().Contain(syncTime.ToLocalTime().ToString("g"));
    }

    [Fact]
    public void Render_ShowsNever_WhenNoSync()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Markup.Should().Contain("Never");
    }

    [Fact]
    public void Connect_CallsCloudSyncService()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(true);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button").Click();

        _cloudSyncServiceMock.Verify(x => x.ConnectAsync(Constants.Sync.DefaultClientId), Times.Once);
    }

    [Fact]
    public void Connect_DisablesButtonWhileConnecting()
    {
        var tcs = new TaskCompletionSource<bool>();
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).Returns(tcs.Task);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button").Click();

        cut.WaitForState(() =>
        {
            var btn = cut.Find("button");
            return btn.TextContent == "Connecting..." && btn.HasAttribute("disabled");
        });

        tcs.SetResult(true);
    }

    [Fact]
    public void Connect_ShowsToast_OnSuccess()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(true);

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.Connected));
    }

    [Fact]
    public void Connect_ShowsToast_OnFailure()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(false);
        _cloudSyncServiceMock.SetupGet(x => x.ClientId).Returns((string?)null);
        _cloudSyncServiceMock.Setup(x => x.ConnectAsync(It.IsAny<string>())).ReturnsAsync(false);

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.AuthFailed));
    }

    [Fact]
    public void Sync_CallsSyncNowAsync()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync()).ReturnsAsync(SyncResult.UpToDate());

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button.sec-btn").Click();

        _cloudSyncServiceMock.Verify(x => x.SyncNowAsync(), Times.Once);
    }

    [Fact]
    public void Sync_DisablesButtonWhileSyncing()
    {
        var tcs = new TaskCompletionSource<SyncResult>();
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync()).Returns(tcs.Task);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button.sec-btn").Click();

        cut.WaitForState(() =>
        {
            var btn = cut.Find("button.sec-btn");
            return btn.TextContent == "Syncing..." && btn.HasAttribute("disabled");
        });

        tcs.SetResult(SyncResult.UpToDate());
    }

    [Fact]
    public void Sync_ShowsToast_OnSuccess()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync()).ReturnsAsync(SyncResult.Pushed());

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.SyncPushSuccess));
    }

    [Fact]
    public void Sync_ShowsToast_OnFailure()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.SyncNowAsync()).ReturnsAsync(SyncResult.Failed("sync error"));

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.sec-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be("sync error"));
    }

    [Fact]
    public void Disconnect_CallsCloudSyncService()
    {
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.DisconnectAsync()).Returns(Task.CompletedTask);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button.danger-btn").Click();

        _cloudSyncServiceMock.Verify(x => x.DisconnectAsync(), Times.Once);
    }

    [Fact]
    public void Disconnect_DisablesButtonWhileDisconnecting()
    {
        var tcs = new TaskCompletionSource();
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.DisconnectAsync()).Returns(tcs.Task);

        var cut = RenderComponent<CloudSyncSettings>();

        cut.Find("button.danger-btn").Click();

        cut.WaitForState(() => cut.Find("button.danger-btn").HasAttribute("disabled"));

        tcs.SetResult();
    }

    [Fact]
    public void Disconnect_ShowsToast()
    {
        var toastMessage = string.Empty;
        _cloudSyncServiceMock.SetupGet(x => x.IsConnected).Returns(true);
        _cloudSyncServiceMock.SetupGet(x => x.LastSyncedAt).Returns((DateTime?)null);
        _cloudSyncServiceMock.Setup(x => x.DisconnectAsync()).Returns(Task.CompletedTask);

        var cut = RenderComponent<CloudSyncSettings>(p =>
            p.Add(x => x.OnShowToast, EventCallback.Factory.Create<string>(this, msg => toastMessage = msg)));

        cut.Find("button.danger-btn").Click();

        cut.WaitForAssertion(() => toastMessage.Should().Be(Constants.SyncMessages.Disconnected));
    }
}

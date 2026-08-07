using FluentAssertions;
using Pomodoro.Web.Models;
using Xunit;

namespace Pomodoro.Web.Tests.Models;

[Trait("Category", "Model")]
public class TimerSettingsExpandTimerMobileTests
{
    [Fact]
    public void Default_IsFalse()
    {
        var settings = new TimerSettings();
        settings.ExpandTimerMobile.Should().BeFalse();
    }

    [Fact]
    public void CanSet_True()
    {
        var settings = new TimerSettings { ExpandTimerMobile = true };
        settings.ExpandTimerMobile.Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenBothDefault_AreEqual()
    {
        var a = new TimerSettings();
        var b = new TimerSettings();
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenExpandDiffers_AreNotEqual()
    {
        var a = new TimerSettings { ExpandTimerMobile = false };
        var b = new TimerSettings { ExpandTimerMobile = true };
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenBothExpanded_AreEqual()
    {
        var a = new TimerSettings { ExpandTimerMobile = true };
        var b = new TimerSettings { ExpandTimerMobile = true };
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_DiffersWhenExpandDiffers()
    {
        var a = new TimerSettings { ExpandTimerMobile = false };
        var b = new TimerSettings { ExpandTimerMobile = true };
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void Clone_PreservesExpandTimerMobile()
    {
        var original = new TimerSettings { ExpandTimerMobile = true };
        var clone = original.Clone();
        clone.ExpandTimerMobile.Should().BeTrue();
    }

    [Fact]
    public void Clone_Default_PreservesFalse()
    {
        var original = new TimerSettings { ExpandTimerMobile = false };
        var clone = original.Clone();
        clone.ExpandTimerMobile.Should().BeFalse();
    }

    [Fact]
    public void OperatorEquality_WhenExpandDiffers_NotEqual()
    {
        var a = new TimerSettings { ExpandTimerMobile = true };
        var b = new TimerSettings { ExpandTimerMobile = false };
        (a != b).Should().BeTrue();
    }
}

using Bunit;
using Pomodoro.Web.Pages;
using Xunit;

namespace Pomodoro.Web.Tests.Pages
{
    [Trait("Category", "Page")]
    public class AboutPageTests : TestContext
    {
        [Fact]
        public void AboutPage_RendersCorrectly()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.NotNull(cut);
        }

        [Fact]
        public void AboutPage_HasCorrectPageRoute()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("/about", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasAboutBodyClass()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("about-body", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasHeroSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("about-hero", cut.Markup);
            Assert.Contains("about-hero-icon", cut.Markup);
            Assert.Contains("about-hero-title", cut.Markup);
            Assert.Contains("about-hero-sub", cut.Markup);
            Assert.Contains("about-hero-desc", cut.Markup);
        }

        [Fact]
        public void AboutPage_DisplaysTitle()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Pomodoro", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasWhatIsSectionToggle()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("What is Pomodoro Technique?", cut.Markup);
            Assert.Contains("collapse-toggle", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasHowItWorksSectionToggle()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("How It Works", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasBenefitsSectionToggle()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Benefits", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasTipsSectionToggle()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Tips", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasDefaultTimesSectionToggle()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Default Timer Settings", cut.Markup);
        }

        [Fact]
        public void AboutPage_CollapsibleSections_NotExpandedByDefault()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert - collapse-body should not be rendered when sections are collapsed
            Assert.DoesNotContain("collapse-body", cut.Markup);
        }

        [Fact]
        public void AboutPage_StepsSection_ExpandsOnClick()
        {
            // Act
            var cut = RenderComponent<About>();
            cut.FindAll(".collapse-toggle")[1].Click();

            // Assert
            Assert.Contains("Choose a Task", cut.Markup);
            Assert.Contains("Set Timer", cut.Markup);
            Assert.Contains("Work with Focus", cut.Markup);
            Assert.Contains("Take a Short Break", cut.Markup);
            Assert.Contains("Repeat", cut.Markup);
        }

        [Fact]
        public void AboutPage_BenefitsGrid_ExpandsOnClick()
        {
            // Act
            var cut = RenderComponent<About>();
            cut.FindAll(".collapse-toggle")[2].Click();

            // Assert
            Assert.Contains("Improved Focus", cut.Markup);
            Assert.Contains("Increased Productivity", cut.Markup);
            Assert.Contains("Better Mental Health", cut.Markup);
            Assert.Contains("Track Progress", cut.Markup);
            Assert.Contains("Time Awareness", cut.Markup);
            Assert.Contains("Reduced Distractions", cut.Markup);
        }

        [Fact]
        public void AboutPage_TipsList_ExpandsOnClick()
        {
            // Act
            var cut = RenderComponent<About>();
            cut.FindAll(".collapse-toggle")[3].Click();

            // Assert
            Assert.Contains("Start Small", cut.Markup);
            Assert.Contains("Protect Your Pomodoro", cut.Markup);
            Assert.Contains("Complete Pomodoro", cut.Markup);
            Assert.Contains("Take Real Breaks", cut.Markup);
            Assert.Contains("Track Your Day", cut.Markup);
            Assert.Contains("Customize Your Times", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasStyleSheetLink()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("about.css", cut.Markup);
        }

        [Fact]
        public void AboutPage_DoesNotHaveCtaSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.DoesNotContain("cta-section", cut.Markup);
            Assert.DoesNotContain("Ready to Boost Your Productivity?", cut.Markup);
            Assert.DoesNotContain("Start Focusing", cut.Markup);
        }
    }
}

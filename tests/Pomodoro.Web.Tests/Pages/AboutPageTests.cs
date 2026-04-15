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
        public void AboutPage_HasAboutPageClass()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("about-page", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasAboutHeader()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Pomodoro", cut.Markup);
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
        public void AboutPage_DisplaysSubtitle()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("A time management method to boost your productivity", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasWhatIsSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("📖 What is Pomodoro Technique?", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasHowItWorksSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("How It Works", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasBenefitsSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Benefits", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasTipsSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Tips", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasDefaultTimesSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Default Timer Settings", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasCallToActionSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Ready to Boost Your Productivity?", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasStartFocusingButton()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Start Focusing", cut.Markup);
        }

        [Fact]
        public void AboutPage_StartFocusingButton_HasCorrectLink()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("href=\"/\"", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasStepsSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Choose a Task", cut.Markup);
            Assert.Contains("Set Timer", cut.Markup);
            Assert.Contains("Work with Focus", cut.Markup);
            Assert.Contains("Take a Short Break", cut.Markup);
            Assert.Contains("Repeat &amp; Long Break", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasBenefitsGrid()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("Improved Focus", cut.Markup);
            Assert.Contains("Increased Productivity", cut.Markup);
            Assert.Contains("Better Mental Health", cut.Markup);
            Assert.Contains("Track Progress", cut.Markup);
            Assert.Contains("Time Awareness", cut.Markup);
            Assert.Contains("Reduced Distractions", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasTipsList()
        {
            // Act
            var cut = RenderComponent<About>();

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
        public void AboutPage_HasAboutContent()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("about-content", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasCtaSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("cta-section", cut.Markup);
        }

        [Fact]
        public void AboutPage_HasInfoSection()
        {
            // Act
            var cut = RenderComponent<About>();

            // Assert
            Assert.Contains("info-section", cut.Markup);
        }
    }
}


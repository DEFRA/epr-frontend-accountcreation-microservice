using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.ViewComponents;
using FrontendAccountCreation.Web.ViewModels.Shared;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Web.UnitTests.ViewComponents;

[TestClass]
public class PhaseBannerTests
{
    [TestMethod]
    public void Invoke_SetsModel()
    {
        // Arrange
        var phaseBannerOptions = new PhaseBannerOptions()
        {
            ApplicationStatus = "Beta", SurveyUrl = "testUrl", Enabled = true
        };
        var options = Options.Create(phaseBannerOptions);
        var component = new PhaseBannerViewComponent(options);

        // Act
        var model = (PhaseBannerModel)component.Invoke().ViewData!.Model!;

        // Assert
        Assert.AreEqual($"PhaseBanner.{phaseBannerOptions.ApplicationStatus}", model.Status);
        Assert.AreEqual(phaseBannerOptions.SurveyUrl, model.Url);
        Assert.AreEqual(phaseBannerOptions.Enabled, model.ShowBanner);
    }
}
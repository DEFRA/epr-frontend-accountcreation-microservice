using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class TypeOfOrganisationTests : OrganisationTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse,PagePath.IsUkMainAddress, PagePath.TradingName, PagePath.TypeOfOrganisation },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task TypeOfOrganisation_Get_WhenSessionContainsProducerType_ReturnsViewWithPrepopulatedModel()
    {
        // Arrange
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader
        };

        // Act
        var result = await _systemUnderTest.TypeOfOrganisation();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ReExTypeOfOrganisationViewModel>().Subject;

        model.ProducerType.Should().Be(ProducerType.SoleTrader);
    }

    [TestMethod]
    public async Task TypeOfOrganisation_Post_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var request = new ReExTypeOfOrganisationViewModel(); // missing ProducerType

        _systemUnderTest.ModelState.AddModelError("ProducerType", "Required");

        // Act
        var result = await _systemUnderTest.TypeOfOrganisation(request);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(request);

        _sessionManagerMock.Verify(x =>
            x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Never);
    }

    [TestMethod]
    public async Task TypeOfOrganisation_Post_ValidModel_SavesSessionAndRedirectsToUkNation()
    {
        // Arrange
        var request = new ReExTypeOfOrganisationViewModel
        {
            ProducerType = ProducerType.Partnership
        };

        // Act
        var result = await _systemUnderTest.TypeOfOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.UkNation));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.ProducerType == ProducerType.Partnership &&
                s.ReExCompaniesHouseSession == null
            )), Times.Once);
    }


    [TestMethod]
    public async Task TypeOfOrganisation_Post_WhenReExManualInputSessionIsNull_InitializesSessionAndSaves()
    {
        // Arrange
        _organisationSession.ReExManualInputSession = null;
        var request = new ReExTypeOfOrganisationViewModel
        {
            ProducerType = ProducerType.Partnership
        };

        // Act
        var result = await _systemUnderTest.TypeOfOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.UkNation));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession != null &&
                s.ReExManualInputSession.ProducerType == ProducerType.Partnership &&
                s.ReExCompaniesHouseSession == null
            )), Times.Once);
    }
}

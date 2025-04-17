using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class TradingNameTests : OrganisationTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent,
                PagePath.TradingName
            ],
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    //todo: GET tests

    [TestMethod]
    public async Task POST_GivenTradingName_ThenRedirectToPartnerOrganisation()
    {
        // Arrange
        var request = new TradingNameViewModel { TradingName = "John Brown Greengrocers" };

        // Act
        var result = await _systemUnderTest.TradingName(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.TradingName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task POST_GivenTradingName_ThenUpdatesSession()
    {
        // Arrange
        var request = new TradingNameViewModel { TradingName = "John Brown Greengrocers" };

        // Act
        await _systemUnderTest.TradingName(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task POST_GivenNoTradingName_ThenSessionNotUpdated()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");

        // Act
        var result = await _systemUnderTest.TradingName(new TradingNameViewModel());

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenNoTradingName_ThenReturnViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");

        // Act
        var result = await _systemUnderTest.TradingName(new TradingNameViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<TradingNameViewModel>();
        //todo: test error
    }

    [TestMethod]
    public async Task POST_GivenNoTradingName_ThenViewHasCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");

        // Act
        var result = await _systemUnderTest.TradingName(new TradingNameViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.IsTradingNameDifferent);
    }
}

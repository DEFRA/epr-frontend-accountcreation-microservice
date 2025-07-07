using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class TradingNameTests : OrganisationPageModelTestBase<TradingName>
{
    private TradingName _tradingName;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        OrganisationSession.Journey =
        [
            PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
            PagePath.ConfirmCompanyDetails, PagePath.UkNation, PagePath.IsTradingNameDifferent,
            PagePath.TradingName
        ];

        _tradingName = new TradingName(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
        {
            PageContext = PageContext
        };
    }

    [TestMethod]
    public async Task OnGet_WhenTradingNameIsNotInSession_ThenTextBoxValueShouldNotBeSet()
    {
        //Act
        await _tradingName.OnGet();

        //Assert
        _tradingName.TextBoxValue.Should().BeNull();
    }

    [TestMethod]
    public async Task OnGet_WhenTradingNameIsInSession_ThenViewIsReturnedWithTradingName()
    {
        //Arrange
        const string tradingName = "Trading name";
        OrganisationSession.TradingName = tradingName;

        //Act
        await _tradingName.OnGet();

        //Assert
        _tradingName.TextBoxValue.Should().Be(tradingName);
    }

    [TestMethod]
    public async Task OnGet_ThenBackLinkIsCorrect()
    {
        //Act
        await _tradingName.OnGet();

        //Assert
        AssertBackLink(_tradingName, PagePath.IsTradingNameDifferent);
    }

    [TestMethod]
    public async Task POST_GivenTradingName_ThenUpdatesSession()
    {
        // Arrange
        _tradingName.TextBoxValue = "John Brown Greengrocers";
        OrganisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
        OrganisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader
        };

        // Act
        await _tradingName.OnPost();

        // Assert
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()),
            Times.Once);
    }

    [TestMethod]
    public async Task OnPost_GivenNoTradingName_ThenSessionNotUpdated()
    {
        // Arrange
        _tradingName.ModelState.AddModelError(nameof(TradingName.TextBoxValue), "Trading name field is required");

        // Act
        await _tradingName.OnPost();

        // Assert
        SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task OnPost_GivenNoTradingName_ThenReturnPage()
    {
        // Arrange
        _tradingName.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");

        // Act
        var result = await _tradingName.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
    }

    /// <summary>
    /// This tests that when the user enters a trading name that is too long,
    /// when we show the page in the errored state, the (too long) trading name they entered is retained
    /// </summary>
    [TestMethod]
    public async Task OnPost_GivenTradingNameTooLong_ThenReturnPageWithUsersBadInput()
    {
        // Arrange
        const string badTradingName = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789";
        _tradingName.TextBoxValue = badTradingName;

        _tradingName.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name must be 170 characters or less");

        // Act
        await _tradingName.OnPost();

        // Assert
        _tradingName.TextBoxValue.Should().Be(badTradingName);
    }

    [TestMethod]
    public async Task OnPost_GivenNoTradingName_ThenViewHasCorrectBackLink()
    {
        // Arrange
        _tradingName.ModelState.AddModelError(nameof(TradingNameViewModel.TradingName), "Trading name field is required");

        // Act
        await _tradingName.OnPost();

        // Assert
        AssertBackLink(_tradingName, PagePath.IsTradingNameDifferent);
    }

    [TestMethod]
    public async Task OnPost_GivenTradingName_CompaniesHouseFlow_ThenRedirectToPartnerOrganisation()
    {
        // Arrange
        _tradingName.TextBoxValue = "John Brown Greengrocers";
        OrganisationSession.OrganisationType = OrganisationType.CompaniesHouseCompany;

        // Act
        var result = await _tradingName.OnPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.IsOrganisationAPartner));
    }

    [TestMethod]
    public async Task POST_GivenTradingName_NonCompaniesHouseFlow_IsSoleTrader_ThenRedirectToTypeOfOrganisation()
    {
        // Arrange
        _tradingName.TextBoxValue = "John Brown Greengrocers";
        OrganisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
        OrganisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader
        };

        // Act
        var result = await _tradingName.OnPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.TypeOfOrganisation));
    }

    [TestMethod]
    public async Task OnPost_GivenTradingName_WithNonUKOrganisationProducerTypeFlow_RedirectsToAddressOverseas()
    {
        // Arrange
        _tradingName.TextBoxValue = "John Brown Greengrocers";
        OrganisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.NonUkOrganisation
        };

        // Act
        var result = await _tradingName.OnPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.AddressOverseas));
    }
}

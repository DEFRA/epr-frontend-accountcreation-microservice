using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class UkNationtests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName, PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress, PagePath.UkNation },
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            ManualInputSession = new ManualInputSession() { BusinessAddress = new Address { Postcode = "NW2 3TB" } },
            IsUserChangingDetails = false
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    [DataRow(ProducerType.NonUkOrganisation)]
    [DataRow(ProducerType.Partnership)]
    [DataRow(ProducerType.UnincorporatedBody)]
    [DataRow(ProducerType.Other)]
    public async Task UkNation_IsManualInputFlow_RedirectsToManualInputRoleInOrganisationPageAndUpdateSession(ProducerType producerType)
    {
        // Arrange
        _accountCreationSessionMock.ManualInputSession.ProducerType = producerType;
        var request = new UkNationViewModel() { UkNation = Nation.England};

        // Act
        var result = await _systemUnderTest.UkNation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should()
            .Be(nameof(AccountCreationController.ManualInputRoleInOrganisation));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    [DataRow(ProducerType.SoleTrader)]
    public async Task UkNation_OrganisationIsSoleTrader_RedirectsToFullNamePageAndUpdateSession(ProducerType producerType)
    {
        // Arrange
        _accountCreationSessionMock.ManualInputSession.ProducerType = producerType;
        var request = new UkNationViewModel() { UkNation = Nation.England};

        // Act
        var result = await _systemUnderTest.UkNation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkNation_OrganisationIsRegisteredWithCompaniesHouse_RedirectsToRoleInOrganisationPageAndUpdateSession()
    {
        // Arrange
        _accountCreationSessionMock.OrganisationType = OrganisationType.CompaniesHouseCompany;
        var request = new UkNationViewModel() { UkNation = Nation.England};

        // Act
        var result = await _systemUnderTest.UkNation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((result as RedirectToActionResult)!).ActionName.Should().Be(nameof(AccountCreationController.RoleInOrganisation));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task UkNation_UkNationSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsSelectBusinessAddress()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(UkNationViewModel.UkNation), "Field is required");

        // Act
        var result = await _systemUnderTest.UkNation(new UkNationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<UkNationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.SelectBusinessAddress);
    }

    [TestMethod]
    public async Task UkNation_UkNationPageIsExited_BackLinkIsSelectBusinessAddress()
    {
        // Act
        var result = await _systemUnderTest.UkNation();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<UkNationViewModel>();
        AssertBackLink(viewResult, PagePath.SelectBusinessAddress);
    }

    [TestMethod]
    public async Task UserNavigatesToUkNationPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation,
                PagePath.TradingName, PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress, PagePath.UkNation,
                PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber, PagePath.CheckYourDetails
            },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.UkNation();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<UkNationViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}

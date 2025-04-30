using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class RoleInOrganisationTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        
        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber, PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation },
            CompaniesHouseSession = new CompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsNone_RedirectsToCannotCreateAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.NoneOfTheAbove};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.CannotCreateAccount));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsDirector_RedirectsToFullName_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Director};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsPartner_RedirectsToFullName_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Partner};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsMember_RedirectsToFullName_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Member};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsCompanySecretary_RedirectsToFullName_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.CompanySecretary};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_OrganisationRoleSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsConfirmCompanyDetails()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(RoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(new RoleInOrganisationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.ConfirmCompanyDetails);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleInOrganisationPageIsExited_BackLinkIsConfirmCompanyDetails()
    {
        //Act
        var result = await _systemUnderTest.RoleInOrganisation();
        
        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();
        AssertBackLink(viewResult, PagePath.ConfirmCompanyDetails);
    }

    [TestMethod]
    public async Task UserNavigatesToRoleInOrganisationPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
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
        var result = await _systemUnderTest.RoleInOrganisation();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}

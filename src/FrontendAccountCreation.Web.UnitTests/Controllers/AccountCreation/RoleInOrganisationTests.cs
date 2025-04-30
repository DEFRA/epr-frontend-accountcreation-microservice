using FluentAssertions;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;
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

    [TestMethod]
    public async Task RoleInOrganisation_WhenCalledWithInviteToken_SetsUpSessionCorrectly()
    {
        // Arrange
        var inviteToken = "test-token";
        var organisationId = "org-123";
        var companyHouseNumber = "12345678";
        var organisationData = new ApprovedPersonOrganisationModel
        {
            OrganisationName = "Test Company",
            SubBuildingName = "Sub Building",
            BuildingName = "Building",
            BuildingNumber = "123",
            Street = "Test Street",
            Town = "Test Town",
            County = "Test County",
            Postcode = "TE1 1ST",
            Country = "United Kingdom"
        };

        _facadeServiceMock.Setup(x => x.GetOrganisationNameByInviteTokenAsync(inviteToken))
            .ReturnsAsync(organisationData);

        _tempDataDictionaryMock.Setup(x => x["InviteToken"]).Returns(inviteToken);
        _tempDataDictionaryMock.Setup(x => x["InvitedOrganisationId"]).Returns(organisationId);
        _tempDataDictionaryMock.Setup(x => x["InvitedCompanyHouseNumber"]).Returns(companyHouseNumber);

        // Act
        var result = await _systemUnderTest.RoleInOrganisation();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<AccountCreationSession>(s => 
                s.IsApprovedUser == true &&
                s.InviteToken == inviteToken &&
                s.OrganisationType == OrganisationType.CompaniesHouseCompany &&
                s.CompaniesHouseSession.IsComplianceScheme == true &&
                s.CompaniesHouseSession.Company.Name == organisationData.OrganisationName &&
                s.CompaniesHouseSession.Company.CompaniesHouseNumber == companyHouseNumber &&
                s.CompaniesHouseSession.Company.OrganisationId == organisationId &&
                s.CompaniesHouseSession.Company.BusinessAddress.SubBuildingName == organisationData.SubBuildingName &&
                s.CompaniesHouseSession.Company.BusinessAddress.BuildingName == organisationData.BuildingName &&
                s.CompaniesHouseSession.Company.BusinessAddress.BuildingNumber == organisationData.BuildingNumber &&
                s.CompaniesHouseSession.Company.BusinessAddress.Street == organisationData.Street &&
                s.CompaniesHouseSession.Company.BusinessAddress.Town == organisationData.Town &&
                s.CompaniesHouseSession.Company.BusinessAddress.County == organisationData.County &&
                s.CompaniesHouseSession.Company.BusinessAddress.Postcode == organisationData.Postcode &&
                s.CompaniesHouseSession.Company.BusinessAddress.Country == organisationData.Country
            )), 
            Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_WhenCalledWithoutInviteToken_OnlyUpdatesBackLink()
    {
        // Arrange
        var session = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RoleInOrganisation }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.RoleInOrganisation();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<AccountCreationSession>(s => 
                s.IsApprovedUser == false &&
                s.InviteToken == null
            )), 
            Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_Post_WhenCompaniesHouseSessionIsNull_CreatesNewSession()
    {
        // Arrange
        var session = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RoleInOrganisation },
            CompaniesHouseSession = null
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var request = new RoleInOrganisationViewModel { RoleInOrganisation = RoleInOrganisation.Director };

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(AccountCreationController.FullName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<AccountCreationSession>(s => 
                s.CompaniesHouseSession != null &&
                s.CompaniesHouseSession.RoleInOrganisation == RoleInOrganisation.Director
            )), 
            Times.Once);
    }
}

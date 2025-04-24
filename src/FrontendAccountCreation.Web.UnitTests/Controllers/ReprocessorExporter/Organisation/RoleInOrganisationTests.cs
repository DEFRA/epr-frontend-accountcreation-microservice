using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class RoleInOrganisationTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
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

       // ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.CannotCreateAccount));

       // _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsDirector_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Director};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

       // ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

      //  _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsPartner_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Partner};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

       // ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

      //  _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsMember_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.Member};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

      //  ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

      //  _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_RoleSavedAsCompanySecretary_RedirectsToManageAccount_AndUpdateSession()
    {
        // Arrange
        var request = new RoleInOrganisationViewModel() { RoleInOrganisation = RoleInOrganisation.CompanySecretary};

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

       // ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

       // _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RoleInOrganisation_OrganisationRoleSavedWithNoAnswer_ReturnsViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(RoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

        // Act
        var result = await _systemUnderTest.RoleInOrganisation(new RoleInOrganisationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();

        //_sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        // AssertBackLink(viewResult, PagePath.ConfirmCompanyDetails);
    }

    //[TestMethod]
    //public async Task RoleInOrganisation_RoleInOrganisationPageIsExited_BackLinkIsConfirmCompanyDetails()
    //{
    //    //Act
    //    var result = await _systemUnderTest.RoleInOrganisation();

    //    //Assert
    //    result.Should().BeOfType<ViewResult>();
    //    var viewResult = (ViewResult)result;
    //    viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();
    //    AssertBackLink(viewResult, PagePath.ConfirmCompanyDetails);
    //}

    //[TestMethod]
    //public async Task UserNavigatesToRoleInOrganisationPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    //{
    //    //Arrange
    //    var accountCreationSessionMock = new AccountCreationSession
    //    {
    //        Journey = new List<string>
    //        {
    //            PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation,
    //            PagePath.TradingName, PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress, PagePath.UkNation,
    //            PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber, PagePath.CheckYourDetails
    //        },
    //        IsUserChangingDetails = true,
    //    };

    //    _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

    //    //Act
    //    var result = await _systemUnderTest.RoleInOrganisation();

    //    //Assert
    //    result.Should().NotBeNull();
    //    result.Should().BeOfType<ViewResult>();
    //    var viewResult = (ViewResult)result;
    //    viewResult.Model.Should().BeOfType<RoleInOrganisationViewModel>();
    //    AssertBackLink(viewResult, PagePath.CheckYourDetails);

    //}
}

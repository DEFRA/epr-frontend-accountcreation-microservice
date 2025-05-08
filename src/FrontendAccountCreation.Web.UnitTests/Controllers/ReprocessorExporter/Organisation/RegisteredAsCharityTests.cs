using FrontendAccountCreation;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.UnitTests;
using FrontendAccountCreation.Web.UnitTests.Controllers;
using FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

using System.Net;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

[TestClass]
public class RegisteredAsCharityTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_RegisteredAsCharity_WithRegulatorDeployment_IsForbidden()
    {
        // Arrange
        SetupBase(DeploymentRoleOptions.RegulatorRoleValue);

        // Act
        var result = await _systemUnderTest.RegisteredAsCharity();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be("Error");
        redirectResult.ActionName.Should().Be("ErrorReEx");
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task Get_RegisteredAsCharity_WithOutRegulatorDeployment_IsAllowed()
    {
        // Act
        var result = await _systemUnderTest.RegisteredAsCharity();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();

        var registeredAsCharityRequestViewModel = (RegisteredAsCharityRequestViewModel)viewResult.Model!;
        registeredAsCharityRequestViewModel.isTheOrganisationCharity.Should().Be(null);
    }

    [TestMethod]
    [DataRow(true, YesNoAnswer.Yes)]
    [DataRow(false, YesNoAnswer.No)]
    public async Task Get_RegisteredAsCharity_ReturnsViewModel_WithSessionData_IsTheOrganisationCharity_As(bool isCharityOrganisation, YesNoAnswer expectedAnswer)
    {
        //Arrange
        var organisationSessionMock = new OrganisationSession
        {
            IsTheOrganisationCharity = isCharityOrganisation
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(organisationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredAsCharity();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();

        var registeredAsCharityRequestViewModel = (RegisteredAsCharityRequestViewModel)viewResult.Model!;
        registeredAsCharityRequestViewModel.isTheOrganisationCharity.Should().Be(expectedAnswer);
    }

    [TestMethod]
    public async Task RegisteredAsCharity_IsNotCharity_RedirectsToRegisteredWithCompaniesHousePage_AndUpdateSession()
    {
        // Arrange
        var request = new RegisteredAsCharityRequestViewModel { isTheOrganisationCharity = YesNoAnswer.No };

        // Act
        var result = await _systemUnderTest.RegisteredAsCharity(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.RegisteredWithCompaniesHouse));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisteredAsCharity_IsCharity_ThenRedirectsToNotAffectedPage_AndUpdateSession()
    {
        // Arrange
        var request = new RegisteredAsCharityRequestViewModel { isTheOrganisationCharity = YesNoAnswer.Yes };

        // Act
        var result = await _systemUnderTest.RegisteredAsCharity(request);

        // Assert       
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.NotAffected));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisteredAsCharity_NoAnswerGiven_ThenReturnViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(RegisteredAsCharityRequestViewModel.isTheOrganisationCharity), "Field is required");

        // Act
        var result = await _systemUnderTest.RegisteredAsCharity(new RegisteredAsCharityRequestViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    }

    [TestMethod]
    public void RedirectToStart_ReturnsView()
    {
        //Act
        var result = _systemUnderTest.RedirectToStart();

        //Arrange
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.RegisteredAsCharity));
    }

    [TestMethod]
    public async Task UserNavigatesToRegisterAsACharityPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var organisationSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber,
                PagePath.CheckYourDetails
            ],
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(organisationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredAsCharity();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();
    }

    [TestMethod]
    public async Task RegisteredAsCharity_RegisteredAsCharityPageIsEntered_BackLinkIsNull()
    {
        //Arrange
        var accountCreationSessionMock = new OrganisationSession
        {
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredAsCharity();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();

        var hasBackLinkKey = viewResult.ViewData.TryGetValue("BackLinkToDisplay", out var gotBackLinkObject);
        hasBackLinkKey.Should().BeFalse();
    }
}

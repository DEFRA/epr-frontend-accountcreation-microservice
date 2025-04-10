namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

using System.Net;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.Controllers.Home;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
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
        registeredAsCharityRequestViewModel.isTheOrganisationCharity.Should().Be(YesNoAnswer.No);
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
    public async Task RegisteredAsCharity_IfUserExistsAndAccountRedirectUrlIsNull_ThenRedirectsToUserAlreadyExistsPage()
    {
        //Arrange
        _facadeServiceMock.Setup(x => x.DoesAccountAlreadyExistAsync()).ReturnsAsync(true);

        // Act
        var result = await _systemUnderTest.RegisteredAsCharity();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(HomeController.UserAlreadyExists));
    }

    [TestMethod]
    public async Task RegisteredAsCharity_IfUserExistsAndHasAccountRedirectUrl_ThenRedirectsToAccountRedirectUrl()
    {
        //Arrange
        var urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        var externalUrl = new ExternalUrlsOptions()
        {
            ExistingUserRedirectUrl = "dummy url"
        };


        _facadeServiceMock.Setup(x => x.DoesAccountAlreadyExistAsync()).ReturnsAsync(true);
        urlsOptionMock.Setup(x => x.Value)
            .Returns(externalUrl);
        var systemUnderTest = new OrganisationController(_sessionManagerMock.Object, _facadeServiceMock.Object, _companyServiceMock.Object,
            _organisationServiceMock.Object, urlsOptionMock.Object, _deploymentRoleOptionMock.Object, _loggerMock.Object);

        // Act
        var result = await systemUnderTest.RegisteredAsCharity();

        // Assert
        result.Should().BeOfType<RedirectResult>();
        ((RedirectResult)result).Url.Should().Be("dummy url");
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
        var accountCreationSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber,
                    PagePath.CheckYourDetails
            },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredAsCharity();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

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

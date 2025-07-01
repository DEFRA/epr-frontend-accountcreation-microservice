using FluentAssertions;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Organisation;
using FrontendAccountCreation.Web.UnitTests;
using FrontendAccountCreation.Web.UnitTests.Controllers;
using FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class RegisteredAsCharityTests : OrganisationPageModelTestBase<RegisteredAsCharity>
{
    private RegisteredAsCharity _registeredAsCharity;
    private Mock<IOptions<DeploymentRoleOptions>> _deploymentRoleOptionsMock;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _deploymentRoleOptionsMock = GetMockDeploymentRoleOptions();

        _registeredAsCharity = new(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
            {
                PageContext = new PageContext
                {
                    HttpContext = _httpContextMock.Object
                }
            };
    }

    //todo: this belongs in its own test class
    //[TestMethod]
    //public void InjectError_ThrowsNotImplementedException()
    //{
    //    // Act & Assert
    //    Assert.ThrowsException<NotImplementedException>(() => _systemUnderTest.InjectError());
    //}

    private Mock<IOptions<DeploymentRoleOptions>> GetMockDeploymentRoleOptions(string? deploymentRole = null)
    {
        var mock = new Mock<IOptions<DeploymentRoleOptions>>();
        mock.Setup(x => x.Value).Returns(new DeploymentRoleOptions
        {
            DeploymentRole = deploymentRole
        });
        return mock;
    }

    [TestMethod]
    public async Task Get_RegisteredAsCharity_WithRegulatorDeployment_IsForbidden()
    {
        // Arrange
        _deploymentRoleOptionsMock = GetMockDeploymentRoleOptions(DeploymentRoleOptions.RegulatorRoleValue);

        // Act
        var result = await _registeredAsCharity.OnGet(_deploymentRoleOptionsMock.Object);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be("Error");
        redirectResult.ActionName.Should().Be("ErrorReEx");
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Get_RegisteredAsCharity_WithOutRegulatorDeployment_IsAllowed(bool useNullMockSession)
    {
        // Arrange
        var org = useNullMockSession ? null : new OrganisationSession();
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult(org));

        // Act
        var result = await _registeredAsCharity.OnGet(_deploymentRoleOptionsMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PageResult>();
    }

    [TestMethod]
    [DataRow(true, "True")]
    [DataRow(false, "False")]
    [DataRow(null, null)]
    public async Task Get_RegisteredAsCharity_ReturnsViewModel_WithSessionData_IsTheOrganisationCharity_As(
        bool? isCharityOrganisation, string? expectedAnswer)
    {
        //Arrange
        var organisationSessionMock = new OrganisationSession
        {
            IsTheOrganisationCharity = isCharityOrganisation
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(organisationSessionMock);

        //Act
        await _registeredAsCharity.OnGet(_deploymentRoleOptionsMock.Object);

        //Assert
        _registeredAsCharity.SelectedValue.Should().Be(expectedAnswer);
    }

    //todo: split into 2
    [TestMethod]
    public async Task RegisteredAsCharity_IsNotCharity_RedirectsToRegisteredWithCompaniesHousePage_AndUpdateSession()
    {
        // Arrange
        _registeredAsCharity.SelectedValue = "False";

        // Act
        var result = await _registeredAsCharity.OnPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.RegisteredWithCompaniesHouse));

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    //[TestMethod]
    //public async Task RegisteredAsCharity_IsCharity_ThenRedirectsToNotAffectedPage_AndUpdateSession()
    //{
    //    // Arrange
    //    var request = new RegisteredAsCharityRequestViewModel { isTheOrganisationCharity = YesNoAnswer.Yes };

    //    // Act
    //    var result = await _systemUnderTest.RegisteredAsCharity(request);

    //    // Assert       
    //    result.Should().BeOfType<RedirectToActionResult>();
    //    ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.NotAffected));

    //    _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    //}

    //[TestMethod]
    //public async Task RegisteredAsCharity_NoAnswerGiven_ThenReturnViewWithError()
    //{
    //    // Arrange
    //    _systemUnderTest.ModelState.AddModelError(nameof(RegisteredAsCharityRequestViewModel.isTheOrganisationCharity), "Field is required");

    //    // Act
    //    var result = await _systemUnderTest.RegisteredAsCharity(new RegisteredAsCharityRequestViewModel());

    //    // Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();

    //    _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    //}

    //[TestMethod]
    //public void RedirectToStart_ReturnsView()
    //{
    //    //Act
    //    var result = _systemUnderTest.RedirectToStart();

    //    //Arrange
    //    result.Should().BeOfType<RedirectToActionResult>();
    //    ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.RegisteredAsCharity));
    //}

    //[TestMethod]
    //public async Task UserNavigatesToRegisterAsACharityPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    //{
    //    //Arrange
    //    var organisationSessionMock = new OrganisationSession
    //    {
    //        Journey =
    //        [
    //            PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
    //            PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber,
    //            PagePath.CheckYourDetails
    //        ],
    //        IsUserChangingDetails = true,
    //    };

    //    _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(organisationSessionMock);

    //    //Act
    //    var result = await _systemUnderTest.RegisteredAsCharity();

    //    //Assert
    //    result.Should().NotBeNull();
    //    result.Should().BeOfType<ViewResult>();
    //    var viewResult = (ViewResult)result;
    //    viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();
    //}

    //[TestMethod]
    //public async Task RegisteredAsCharity_RegisteredAsCharityPageIsEntered_BackLinkIsNull()
    //{
    //    //Arrange
    //    var accountCreationSessionMock = new OrganisationSession
    //    {
    //        IsUserChangingDetails = false,
    //    };

    //    _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

    //    //Act
    //    var result = await _systemUnderTest.RegisteredAsCharity();

    //    //Assert
    //    result.Should().NotBeNull();
    //    result.Should().BeOfType<ViewResult>();
    //    var viewResult = (ViewResult)result;
    //    viewResult.Model.Should().BeOfType<RegisteredAsCharityRequestViewModel>();

    //    var hasBackLinkKey = viewResult.ViewData.TryGetValue("BackLinkToDisplay", out var gotBackLinkObject);
    //    hasBackLinkKey.Should().BeFalse();
    //}
}

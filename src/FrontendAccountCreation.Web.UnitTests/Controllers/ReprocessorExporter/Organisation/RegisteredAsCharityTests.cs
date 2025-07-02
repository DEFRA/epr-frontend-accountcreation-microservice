using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Organisation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

        //todo: if this works, move into base class
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        //_registeredAsCharity.ViewData = viewData;
        //_registeredAsCharity.PageContext.ViewData = viewData;

        //todo: add helper in base for this generic?
        _registeredAsCharity = new(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
            {
                //ViewData = viewData,
                PageContext = new PageContext
                {
                    HttpContext = _httpContextMock.Object,
                    ViewData = viewData
                }
            };
    }

    private Mock<IOptions<DeploymentRoleOptions>> GetMockDeploymentRoleOptions(string? deploymentRole = null)
    {
        var mock = new Mock<IOptions<DeploymentRoleOptions>>();
        mock.Setup(x => x.Value).Returns(new DeploymentRoleOptions
        {
            DeploymentRole = deploymentRole
        });
        return mock;
    }

    //todo: remove RegisteredAsCharity from names, use OnGet_ and OnPost_ prefixes

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

    //todo: create a working test: note that no back link is currently set. add it and create a bug if necessary
    [Ignore("this test doesn't actually test what it says it does (leave for now as probably going to replace these tests with the yes/no standard test)")]
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

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(organisationSessionMock);

        //Act
        var result = await _registeredAsCharity.OnGet(_deploymentRoleOptionsMock.Object);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PageResult>();
    }

    [TestMethod]
    public async Task RegisteredAsCharity_RegisteredAsCharityPageIsEntered_BackLinkIsNull()
    {
        //Arrange
        var accountCreationSessionMock = new OrganisationSession
        {
            IsUserChangingDetails = false,
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        await _registeredAsCharity.OnGet(_deploymentRoleOptionsMock.Object);

        //Assert
        _registeredAsCharity.ViewData["BackLinkToDisplay"].Should().BeNull();
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

    //todo: split into 2
    [TestMethod]
    public async Task RegisteredAsCharity_IsCharity_ThenRedirectsToNotAffectedPage_AndUpdateSession()
    {
        // Arrange
        _registeredAsCharity.SelectedValue = "True";

        // Act
        var result = await _registeredAsCharity.OnPost();

        // Assert       
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.NotAffected));

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisteredAsCharity_NoAnswerGiven_ThenReturnViewWithError()
    {
        // Arrange
        _registeredAsCharity.ModelState.AddModelError(nameof(RegisteredAsCharityRequestViewModel.isTheOrganisationCharity), "Field is required");

        // Act
        var result = await _registeredAsCharity.OnPost();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PageResult>();

        _registeredAsCharity.ModelState.Should().ContainKey(nameof(RegisteredAsCharityRequestViewModel.isTheOrganisationCharity));
        _registeredAsCharity.ModelState[nameof(RegisteredAsCharityRequestViewModel.isTheOrganisationCharity)].Errors.Should().ContainSingle();

        SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    }
}

using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
using Microsoft.Extensions.Localization;

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

        _registeredAsCharity = new RegisteredAsCharity(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
        {
            PageContext = PageContext
        };
    }

    private static Mock<IOptions<DeploymentRoleOptions>> GetMockDeploymentRoleOptions(string? deploymentRole = null)
    {
        var mock = new Mock<IOptions<DeploymentRoleOptions>>();
        mock.Setup(x => x.Value).Returns(new DeploymentRoleOptions
        {
            DeploymentRole = deploymentRole
        });
        return mock;
    }

    [TestMethod]
    public async Task OnGet_RegisteredAsCharity_WithRegulatorDeployment_IsForbidden()
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
    public async Task OnGet_RegisteredAsCharity_WithOutRegulatorDeployment_IsAllowed(bool useNullMockSession)
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
    public async Task OnGet_RegisteredAsCharity_ReturnsViewModel_WithSessionData_IsTheOrganisationCharity_As(
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

    //to-do: create a working test: note that no back link is currently set. add it and create a bug if necessary
    [Ignore("this test doesn't actually test what it says it does (leave for now as probably going to replace these tests with the yes/no standard test)")]
    [TestMethod]
    public async Task OnGet_UserNavigatesToRegisterAsACharityPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
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
        await _registeredAsCharity.OnGet(_deploymentRoleOptionsMock.Object);

        //Assert
        _registeredAsCharity.ViewData["BackLinkToDisplay"].Should().Be("todo");
    }

    [TestMethod]
    public async Task OnGet_RegisteredAsCharity_RegisteredAsCharityPageIsEntered_BackLinkIsNull()
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
    public async Task OnPost_RegisteredAsCharity_IsNotCharity_RedirectsToRegisteredWithCompaniesHousePage_AndUpdateSession()
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
    public async Task OnPost_RegisteredAsCharity_IsCharity_ThenRedirectsToNotAffectedPage_AndUpdateSession()
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
    public async Task OnPost_RegisteredAsCharity_NoAnswerGiven_ThenReturnViewWithError()
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

    [TestMethod]
    public void Radios_ShouldReturnYesNoRadios()
    {
        // Act
        var radios = _registeredAsCharity.Radios.ToList();

        // Assert
        radios.Should().HaveCount(2);
        radios[0].Value.Should().Be("True");
        radios[1].Value.Should().Be("False");
        //to-do: setup shared localizer and check localized strings
    }

    [TestMethod]
    public void Legend_ShouldReturnLocalizedQuestion()
    {
        // Arrange
        LocalizerMock.Setup(l => l["RegisteredAsCharity.Question"])
            .Returns(new LocalizedString("RegisteredAsCharity.Question", "Test question string"));

        // Act
        var legend = _registeredAsCharity.Legend;

        // Assert
        legend.Should().Be("Test question string");
    }

    [TestMethod]
    public void Hint_ShouldReturnLocalizedDescription()
    {
        // Arrange
        LocalizerMock.Setup(l => l["RegisteredAsCharity.Description"])
            .Returns(new LocalizedString("RegisteredAsCharity.Description", "Test hint string"));

        // Act
        var hint = _registeredAsCharity.Hint;

        // Assert
        hint.Should().Be("Test hint string");
    }
}

using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class TeamRoleTests : OrganisationTestBase
{
    private OrganisationSession _orgSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity,
                PagePath.RegisteredWithCompaniesHouse,
                PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails,
                PagePath.RoleInOrganisation,
                PagePath.ManageAccountPerson,
                PagePath.TeamRole
            ]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task GET_BackLinkIsManageAccountPerson()
    {
        //Act
        var result = await _systemUnderTest.TeamRole();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.ManageAccountPerson);
    }

    [TestMethod]
    public async Task GET_CurrentApprovedPersonNotInSession_CorrectViewModelIsReturnedInTheView()
    {
        //Act
        var result = await _systemUnderTest.TeamRole();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TeamRoleViewModel>();
        var viewModel = (TeamRoleViewModel?)viewResult.Model;
        viewModel!.TeamRoleInOrganisation.Should().BeNull();
    }

    [TestMethod]
    [DataRow(TeamRoleInOrganisation.Director, TeamRoleInOrganisation.Director)]
    [DataRow(TeamRoleInOrganisation.CompanySecretary, TeamRoleInOrganisation.CompanySecretary)]
    public async Task GET_CurrentApprovedPersonInSession_CorrectViewModelIsReturnedInTheView(TeamRoleInOrganisation teamRoleInSession, TeamRoleInOrganisation expectedTeamRoleInViewModel)
    {
        //Arrange
        _orgSessionMock.CurrentApprovedPerson = new ApprovedPerson
        {
            TeamRoleInOrganisation = teamRoleInSession
        };

        //Act
        var result = await _systemUnderTest.TeamRole();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TeamRoleViewModel>();
        var viewModel = (TeamRoleViewModel?)viewResult.Model;
        viewModel!.TeamRoleInOrganisation.Should().Be(expectedTeamRoleInViewModel);
    }

    [TestMethod]
    [DataRow(TeamRoleInOrganisation.Director, true, TeamRoleInOrganisation.Director)]
    [DataRow(TeamRoleInOrganisation.Director, false, TeamRoleInOrganisation.Director)]
    [DataRow(TeamRoleInOrganisation.CompanySecretary, true, TeamRoleInOrganisation.CompanySecretary)]
    [DataRow(TeamRoleInOrganisation.CompanySecretary, false, TeamRoleInOrganisation.CompanySecretary)]
    public async Task POST_UserSelectsDirectorOrSecretary_SessionUpdatedCorrectly(
        TeamRoleInOrganisation userAnswer,
        bool invitation,
        TeamRoleInOrganisation expectedTeamRoleInSession)
    {
        // Arrange
        var request = new TeamRoleViewModel { TeamRoleInOrganisation = userAnswer };

        // Act
        await _systemUnderTest.TeamRole(request, invitation);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
            It.Is<OrganisationSession>(os => os.CurrentApprovedPerson!.TeamRoleInOrganisation == expectedTeamRoleInSession)),
            Times.Once);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task POST_UserSelectsNothing_SessionNotUpdated(bool invitation)
    {
        // Arrange
        var request = new TeamRoleViewModel { TeamRoleInOrganisation = null };
        _systemUnderTest.ModelState.AddModelError("TeamRoleInOrganisation", "Select their role as shown on Companies House");

        // Act
        await _systemUnderTest.TeamRole(request, invitation);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
                It.IsAny<OrganisationSession>()),
            Times.Never);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task POST_UserSelectsNothing_ViewIsReturnedWithCorrectModel(bool invitation)
    {
        // Arrange
        var request = new TeamRoleViewModel { TeamRoleInOrganisation = null };
        _systemUnderTest.ModelState.AddModelError("TeamRoleInOrganisation", "Select their role as shown on Companies House");

        // Act
        var result = await _systemUnderTest.TeamRole(request, invitation);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TeamRoleViewModel>();
        var viewModel = (TeamRoleViewModel?)viewResult.Model;
        viewModel!.TeamRoleInOrganisation.Should().BeNull();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task POST_UserSelectsNothing_ViewHasCorrectBackLink(bool invitation)
    {
        // Arrange
        var request = new TeamRoleViewModel { TeamRoleInOrganisation = null };
        _systemUnderTest.ModelState.AddModelError("TeamRoleInOrganisation", "Select their role as shown on Companies House");

        // Act
        var result = await _systemUnderTest.TeamRole(request, invitation);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.ManageAccountPerson);
    }

    [TestMethod]
    [DataRow(true, nameof(OrganisationController.TeamInvite))]
    [DataRow(false, nameof(OrganisationController.CheckYourAnswers))]
    public async Task POST_UserSelectsInvitationYesOrNo_UserIsRedirected(bool invitation, string expectedRedirect)
    {
        // Arrange
        var request = new TeamRoleViewModel { TeamRoleInOrganisation = TeamRoleInOrganisation.Director };

        // Act
        var result = await _systemUnderTest.TeamRole(request, invitation);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
    }
}
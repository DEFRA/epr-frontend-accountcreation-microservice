using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class TeamMemberRoleInOrganisationTests : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity,
                PagePath.RegisteredWithCompaniesHouse,
                PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails,
                PagePath.RoleInOrganisation,
                "Pagebefore",
                ReExPagePath.TeamMemberRoleInOrganisation,
            },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Get_WithExistingTeamMemberId_ReturnsViewWithRole()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var existingRole = ReExTeamMemberRole.Director;

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = teamMemberId,
                    Role = existingRole
                }
            }
        };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(teamMemberId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as TeamMemberRoleInOrganisationViewModel;
        model.Should().NotBeNull();
        model!.RoleInOrganisation.Should().Be(existingRole);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Get_WithInvalidTeamMemberId_ReturnsEmptyView()
    {
        // Arrange
        var invalidTeamMemberId = Guid.NewGuid();

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = Guid.NewGuid(),
                    Role = ReExTeamMemberRole.Director
                }
            }
        };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(invalidTeamMemberId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Get_WithNoTeamMembers_ReturnsEmptyView()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>()
        };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(teamMemberId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithExistingTeamMember_UpdatesRoleAndRedirectsToCheckInvitationDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var newRole = ReExTeamMemberRole.CompanySecretary;

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = teamMemberId,
                    Role = ReExTeamMemberRole.Director
                }
            }
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            Id = teamMemberId,
            RoleInOrganisation = newRole
        };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.TeamMembersCheckInvitationDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.TeamMembers[0].Role == newRole &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Id == teamMemberId
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithNewTeamMember_CreatesNewMemberAndRedirectsToDetails()
    {
        // Arrange
        var newRole = ReExTeamMemberRole.Director;

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>()
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = newRole
        };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.TeamMembers.Count == 1 &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Role == newRole &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Id != Guid.Empty
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithNullCompaniesHouseSession_CreatesNewSessionAndMember()
    {
        // Arrange
        var newRole = ReExTeamMemberRole.Director;
        _orgSessionMock.ReExCompaniesHouseSession = null;

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = newRole
        };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession != null &&
                s.ReExCompaniesHouseSession.TeamMembers.Count == 1 &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Role == newRole &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Id != Guid.Empty
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithInvalidModel_ReturnsViewWithError()
    {
        // Arrange
        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = null
        };

        _systemUnderTest.ModelState.AddModelError(nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as TeamMemberRoleInOrganisationViewModel;
        model.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
        AssertBackLink(viewResult, "Pagebefore");

        // Verify ModelState contains the error
        _systemUnderTest.ModelState.IsValid.Should().BeFalse();
        _systemUnderTest.ModelState[nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation)]
            .Errors.Should()
            .Contain(e => e.ErrorMessage == "Field is required");
    }
}
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
public class NonCompaniesHousePartnershipTeamMemberRoleTests : ApprovedPersonTestBase
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
                PagePath.NonCompaniesHousePartnershipAddApprovedPerson,
                PagePath.NonCompaniesHousePartnershipTheirRole
            },
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = Core.Sessions.ProducerType.Partnership
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Get_WithExistingTeamMemberId_ReturnsViewWithRole()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var existingRole = ReExTeamMemberRole.Director;

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
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
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(teamMemberId);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>().Which;
        model.Should().NotBeNull();
        model.RoleInOrganisation.Should().Be(existingRole);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Get_WithInvalidTeamMemberId_ReturnsEmptyView()
    {
        // Arrange
        var invalidTeamMemberId = Guid.NewGuid();

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers =
            [
                new ReExCompanyTeamMember
                {
                    Id = Guid.NewGuid(), // different from invalidTeamMemberId
                    Role = ReExTeamMemberRole.Director
                }
            ]
        };
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(invalidTeamMemberId);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;

        // Instead of expecting null, assert it's an empty model
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
        var model = (TeamMemberRoleInOrganisationViewModel)viewResult.Model;
        model.Id.Should().BeNull();
        model.RoleInOrganisation.Should().BeNull();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Get_WithNoTeamMembers_ReturnsEmptyView()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>()
        };
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(teamMemberId);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>().Which;
        model.Id.Should().BeNull();
        model.RoleInOrganisation.Should().BeNull();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Get_WithExistingMemberAndPartnership_ReturnsPartnershipView()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var role = ReExTeamMemberRole.PartnerDirector;

        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = teamMemberId,
                    Role = role
                }
            }
        };

        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(teamMemberId);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>().Which;
        model.Should().NotBeNull();
        model.RoleInOrganisation.Should().Be(role);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Get_NullId_ReturnsDefaultView()
    {
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole();

        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Get_InvalidId_WithPartnership_ReturnsPartnershipView()
    {
        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExManualInputSession.TeamMembers = new List<ReExCompanyTeamMember>();

        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(Guid.NewGuid());

        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole();
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        viewResult.ViewName.Should().BeNull();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Get_WithValidIdNotInList_SetsFocusIdAndReturnsEmptyViewModel()
    {
        // Arrange
        var nonMatchingId = Guid.NewGuid();
        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = Guid.NewGuid(), // different ID
                    Role = ReExTeamMemberRole.Director
                }
            }
        };
        _tempDataDictionaryMock.Setup(x => x["FocusId"]).Returns(nonMatchingId);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        viewResult.ViewName.Should().BeNull(); // default view
        var model = viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>().Which;
        model.Should().NotBeNull();
        model.Id.Should().BeNull();
        model.RoleInOrganisation.Should().BeNull();
        _systemUnderTest.GetFocusId().Should().Be(nonMatchingId); // confirms SetFocusId was called
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithExistingTeamMember_UpdatesRoleAndRedirectsToCheckInvitationDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = teamMemberId,
                    Role = ReExTeamMemberRole.Director,
                    Email = "director@outlook.com"
                }
            }
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            Id = teamMemberId,
            RoleInOrganisation = ReExTeamMemberRole.CompanySecretary
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers[0].Role == ReExTeamMemberRole.CompanySecretary &&
                s.ReExManualInputSession.TeamMembers[0].Id == teamMemberId
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithNewTeamMember_CreatesNewMemberAndRedirectsToDetails()
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>()
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers.Count == 1 &&
                s.ReExManualInputSession.TeamMembers[0].Role == ReExTeamMemberRole.Director &&
                s.ReExManualInputSession.TeamMembers[0].Id != Guid.Empty
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithNullCompaniesHouseSession_CreatesNewSessionAndMember()
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession = null;

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession != null &&
                s.ReExManualInputSession.TeamMembers.Count == 1 &&
                s.ReExManualInputSession.TeamMembers[0].Role == ReExTeamMemberRole.Director &&
                s.ReExManualInputSession.TeamMembers[0].Id != Guid.Empty
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithInvalidModel_ReturnsViewWithError()
    {
        // Arrange
        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = null
        };

        _systemUnderTest.ModelState.AddModelError(nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        var model = viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>().Which;
        model.Should().NotBeNull();

        // Verify ModelState contains the error
        _systemUnderTest.ModelState.IsValid.Should().BeFalse();
        _systemUnderTest.ModelState[nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation)]
            .Errors.Should()
            .Contain(e => e.ErrorMessage == "Field is required");
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithCompanySecretaryRole_CreatesNewMemberAndRedirectsToDetails()
    {
        // Arrange
        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>()
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = ReExTeamMemberRole.CompanySecretary
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers.Count == 1 &&
                s.ReExManualInputSession.TeamMembers[0].Role == ReExTeamMemberRole.CompanySecretary &&
                s.ReExManualInputSession.TeamMembers[0].Id != Guid.Empty
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithDirectorRole_CreatesNewMemberAndRedirectsToDetails()
    {
        // Arrange
        var existingTeamMembers = new List<ReExCompanyTeamMember>
        {
            new ReExCompanyTeamMember
            {
                Id = Guid.NewGuid(),
                Role = ReExTeamMemberRole.CompanySecretary
            }
        };

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = existingTeamMembers
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers.Count == 2 &&
                s.ReExManualInputSession.TeamMembers[0].Role == ReExTeamMemberRole.CompanySecretary &&
                s.ReExManualInputSession.TeamMembers[1].Role == ReExTeamMemberRole.Director &&
                s.ReExManualInputSession.TeamMembers[1].Id != Guid.Empty
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithCompanySecretaryRole_UpdatesExistingMemberAndRedirectsToCheckInvitationDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var existingTeamMembers = new List<ReExCompanyTeamMember>
        {
            new ReExCompanyTeamMember
            {
                Id = teamMemberId,
                Role = ReExTeamMemberRole.Director,
                Email = "director@gmail.com"
            },
            new ReExCompanyTeamMember
            {
                Id = Guid.NewGuid(),
                Role = ReExTeamMemberRole.Director,
                Email = "spielberger@gmail.com"
            }
        };

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = existingTeamMembers
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            Id = teamMemberId,
            RoleInOrganisation = ReExTeamMemberRole.CompanySecretary
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers.Count == 2 &&
                s.ReExManualInputSession.TeamMembers[0].Role == ReExTeamMemberRole.CompanySecretary &&
                s.ReExManualInputSession.TeamMembers[0].Id == teamMemberId &&
                s.ReExManualInputSession.TeamMembers[1].Role == ReExTeamMemberRole.Director
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithDirectorRole_UpdatesExistingMemberAndRedirectsToCheckInvitationDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = teamMemberId,
                    Role = ReExTeamMemberRole.CompanySecretary,
                    Email = "companysecretary@gmail.com"
                }
            }
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            Id = teamMemberId,
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers[0].Role == ReExTeamMemberRole.Director &&
                s.ReExManualInputSession.TeamMembers[0].Id == teamMemberId
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithNoneRole_RemovesExistingMemberAndRedirectsToCheckYourDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = teamMemberId,
                    Role = ReExTeamMemberRole.Director,
                    Email = "director@outlook.com"
                }
            }
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            Id = teamMemberId,
            RoleInOrganisation = ReExTeamMemberRole.None
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(request);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.CheckYourDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
                It.IsAny<ISession>(),
                It.Is<OrganisationSession>(s =>
                    s.ReExManualInputSession.TeamMembers.TrueForAll(x => x.Id != teamMemberId)
                )),
            Times.Once);
    }

    [TestMethod]
    // As the test name implies, this test needs to be rewritten when the target page is ready
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WithNoneRoleAndNonExistingMember_RedirectsToCannotBeInvited()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>() // Empty list
        };

        var model = new TeamMemberRoleInOrganisationViewModel
        {
            Id = nonExistentId,
            RoleInOrganisation = ReExTeamMemberRole.None
        };

        // Act
        var act = async () => await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(model);

        // Assert
        act.Should().ThrowAsync<NotImplementedException>().WithMessage("You cannot invite this person to be an approved person");
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WhenGivenEmptyId_TreatedAsNew_And_Redirects()
    {
        var model = new TeamMemberRoleInOrganisationViewModel
        {
            Id = Guid.Empty,
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(model);
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WhenGivenNewApprovedPerson_Redirects()
    {
        _orgSessionMock.ReExManualInputSession.TeamMembers = null;

        var model = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(model);
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));
    }

    [TestMethod]
    // As the name implies, the test needs rewriting when the target page is developed
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_NoneRole_NullId_RedirectsToCannotBeInvited()
    {
        var model = new TeamMemberRoleInOrganisationViewModel
        {
            Id = null,
            RoleInOrganisation = ReExTeamMemberRole.None
        };

        var act = async () => await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(model);

        // Assert
        act.Should().ThrowAsync<NotImplementedException>().WithMessage("You cannot invite this person to be an approved person");
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_InvalidModel_WhenPartnership_ReturnsView()
    {
        var model = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = null
        };
        _systemUnderTest.ModelState.AddModelError("TeamMemberRoleInOrganisation.ErrorMessage", "Required");

        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(model);
        var viewResult = result.Should().BeOfType<ViewResult>().Which;
        viewResult.ViewName.Should().BeNull();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipTeamMemberRole_Post_WhenGivemDuplicateIds_OnlyFirstUpdated_AndRedirects()
    {
        var duplicateId = Guid.NewGuid();
        _orgSessionMock.ReExManualInputSession.TeamMembers = new List<ReExCompanyTeamMember>
        {
            new ReExCompanyTeamMember { Id = duplicateId, Role = ReExTeamMemberRole.Director, Email = "director@outlook.com" },
            new ReExCompanyTeamMember { Id = duplicateId, Role = ReExTeamMemberRole.Director, Email = "director@gmail.com" }
        };

        var model = new TeamMemberRoleInOrganisationViewModel
        {
            Id = duplicateId,
            RoleInOrganisation = ReExTeamMemberRole.CompanySecretary
        };

        var result = await _systemUnderTest.NonCompaniesHousePartnershipTeamMemberRole(model);

        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));

        _orgSessionMock.ReExManualInputSession.TeamMembers[0].Role.Should().Be(ReExTeamMemberRole.CompanySecretary);
        _orgSessionMock.ReExManualInputSession.TeamMembers[1].Role.Should().Be(ReExTeamMemberRole.Director);
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberDetailsEdit_RedirectsToDetailsWithId()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetailsEdit(teamMemberId);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirect.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails));
        redirect.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(teamMemberId);
    }
}
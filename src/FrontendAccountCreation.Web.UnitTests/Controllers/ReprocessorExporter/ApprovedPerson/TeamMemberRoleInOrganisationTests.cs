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
                PagePath.TeamMemberRoleInOrganisation,
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
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(teamMemberId);

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

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
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        // Instead of expecting null, assert it's an empty model
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
        var model = (TeamMemberRoleInOrganisationViewModel)viewResult.Model;
        model.Id.Should().BeNull();
        model.RoleInOrganisation.Should().BeNull();
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
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(teamMemberId);

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
        viewResult.Model.As<TeamMemberRoleInOrganisationViewModel>().Id.Should().BeNull();
        viewResult.Model.As<TeamMemberRoleInOrganisationViewModel>().RoleInOrganisation.Should().BeNull();
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
                    Role = ReExTeamMemberRole.Director,
                    Email = "director@outlook.com"
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
        // AssertBackLink(viewResult, "Pagebefore");

        // Verify ModelState contains the error
        _systemUnderTest.ModelState.IsValid.Should().BeFalse();
        _systemUnderTest.ModelState[nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation)]
            .Errors.Should()
            .Contain(e => e.ErrorMessage == "Field is required");
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithCompanySecretaryRole_CreatesNewMemberAndRedirectsToDetails()
    {
        // Arrange
        var newRole = ReExTeamMemberRole.CompanySecretary;

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
    public async Task TeamMemberRoleInOrganisation_Post_WithDirectorRole_CreatesNewMemberAndRedirectsToDetails()
    {
        // Arrange
        var newRole = ReExTeamMemberRole.Director;
        var existingTeamMembers = new List<ReExCompanyTeamMember>
        {
            new ReExCompanyTeamMember
            {
                Id = Guid.NewGuid(),
                Role = ReExTeamMemberRole.CompanySecretary
            }
        };

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = existingTeamMembers
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
                s.ReExCompaniesHouseSession.TeamMembers.Count == 2 &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Role == ReExTeamMemberRole.CompanySecretary &&
                s.ReExCompaniesHouseSession.TeamMembers[1].Role == newRole &&
                s.ReExCompaniesHouseSession.TeamMembers[1].Id != Guid.Empty
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithCompanySecretaryRole_UpdatesExistingMemberAndRedirectsToCheckInvitationDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var newRole = ReExTeamMemberRole.CompanySecretary;
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

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = existingTeamMembers
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
                s.ReExCompaniesHouseSession.TeamMembers.Count == 2 &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Role == newRole &&
                s.ReExCompaniesHouseSession.TeamMembers[0].Id == teamMemberId &&
                s.ReExCompaniesHouseSession.TeamMembers[1].Role == ReExTeamMemberRole.Director
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithDirectorRole_UpdatesExistingMemberAndRedirectsToCheckInvitationDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var newRole = ReExTeamMemberRole.Director;

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
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
    public async Task TeamMemberRoleInOrganisation_Post_WithNoneRole_RemovesExistingMemberAndRedirectsToCheckYourDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
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
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.CheckYourDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
                It.IsAny<ISession>(),
                It.Is<OrganisationSession>(s =>
                    s.ReExCompaniesHouseSession.TeamMembers.All(x => x.Id != teamMemberId)
                )),
            Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_WithNoneRoleAndNonExistingMember_RedirectsToCannotBeInvited()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>() // Empty list
        };

        var request = new TeamMemberRoleInOrganisationViewModel
        {
            Id = nonExistentId,
            RoleInOrganisation = ReExTeamMemberRole.None
        };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.PersonCanNotBeInvited));
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Get_WithExistingMemberAndPartnership_ReturnsPartnershipView()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var role = ReExTeamMemberRole.PartnerDirector;

        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
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
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().Be("ApprovedPersonPartnershipRole");
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
        ((TeamMemberRoleInOrganisationViewModel)viewResult.Model!).RoleInOrganisation.Should().Be(role);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Get_NullId_ReturnsDefaultView()
    {
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_GuidEmptyId_TreatedAsNew()
    {
        var model = new TeamMemberRoleInOrganisationViewModel
        {
            Id = Guid.Empty,
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(model);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberDetails));
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_NullTeamMembers_InitialisesAndAdds()
    {
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = null;

        var model = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = ReExTeamMemberRole.Director
        };

        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(model);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberDetails));
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_NoneRole_NullId_RedirectsToCannotBeInvited()
    {
        var model = new TeamMemberRoleInOrganisationViewModel
        {
            Id = null,
            RoleInOrganisation = ReExTeamMemberRole.None
        };

        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(model);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.PersonCanNotBeInvited));
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Get_InvalidId_WithPartnership_ReturnsPartnershipView()
    {
        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = new List<ReExCompanyTeamMember>();

        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(Guid.NewGuid());

        var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).ViewName.Should().Be("ApprovedPersonPartnershipRole");
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_InvalidModel_WhenPartnership_ReturnsPartnershipView()
    {
        _orgSessionMock.IsOrganisationAPartnership = true;

        var model = new TeamMemberRoleInOrganisationViewModel
        {
            RoleInOrganisation = null
        };
        _systemUnderTest.ModelState.AddModelError("RoleInOrganisation", "Required");

        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(model);

        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).ViewName.Should().Be("ApprovedPersonPartnershipRole");
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_Post_DuplicateIds_OnlyFirstUpdated()
    {
        var duplicateId = Guid.NewGuid();
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = new List<ReExCompanyTeamMember>
        {
            new ReExCompanyTeamMember { Id = duplicateId, Role = ReExTeamMemberRole.Director, Email = "director@outlook.com" },
            new ReExCompanyTeamMember { Id = duplicateId, Role = ReExTeamMemberRole.CompanySecretary, Email = "companysecretary@gmail.com" }
        };

        var model = new TeamMemberRoleInOrganisationViewModel
        {
            Id = duplicateId,
            RoleInOrganisation = ReExTeamMemberRole.CompanySecretary
        };

        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(model);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMembersCheckInvitationDetails));

        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers[0].Role.Should().Be(ReExTeamMemberRole.CompanySecretary);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers[1].Role.Should().Be(ReExTeamMemberRole.CompanySecretary);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisationAdd_Get_RedirectsTo_TeamMemberRoleInOrganisation()
    {
        var result = await _systemUnderTest.TeamMemberRoleInOrganisationAdd();

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisationEdit_Get_RedirectsTo_TeamMemberRoleInOrganisation()
    {
        var result = await _systemUnderTest.TeamMemberRoleInOrganisationEdit(Guid.NewGuid());

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task CanNotInviteThisPerson_Get_ReturnsViewWithCorrectId()
    {
        // Arrange
        var testId = Guid.NewGuid();

        // Act
        var result = await _systemUnderTest.CanNotInviteThisPerson(testId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var viewModel = viewResult.Model.Should().BeOfType<LimitedPartnershipPersonCanNotBeInvitedViewModel>().Subject;
        viewModel.Id.Should().Be(testId);
    }

    [TestMethod]
    public void CanNotInviteThisPerson_Post_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new LimitedPartnershipPersonCanNotBeInvitedViewModel
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = _systemUnderTest.CanNotInviteThisPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be("CheckYourDetails");
        redirectResult.ControllerName.Should().Be("AccountCreation");
    }

}
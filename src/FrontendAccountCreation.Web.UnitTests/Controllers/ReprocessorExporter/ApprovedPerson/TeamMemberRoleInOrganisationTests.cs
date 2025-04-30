using FluentAssertions;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.UnitTests;
using FrontendAccountCreation.Web.UnitTests.Controllers;
using FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

/// <summary>
/// These tests should fail when the correct pages are plumbed in.
/// </summary>
[TestClass]
public class TeamMemberRoleInOrganisationTests : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock = null!;

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
                "Pagebefore", // replace when page is developed
                ReExPagePath.TeamMemberRoleInOrganisation,
            },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    [DataRow(ReExTeamMemberRole.CompanySecretary)]
    [DataRow(ReExTeamMemberRole.Director)]
    public async Task TeamMemberRoleInOrganisation_WithInvitation_Redirects_AndUpdateSession(ReExTeamMemberRole role)
    {
        // Arrange
        var request = new TeamMemberRoleInOrganisationViewModel() { RoleInOrganisation = role };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_OrganisationRoleSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsConfirmCompanyDetails()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(new TeamMemberRoleInOrganisationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
        AssertBackLink(viewResult, "Pagebefore");
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_PageIsExited_BackLinkIsPageBefore()
    {
        //Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(Guid.NewGuid());

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
        AssertBackLink(viewResult, "Pagebefore");
    }
}

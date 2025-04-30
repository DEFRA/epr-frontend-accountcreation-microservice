using FluentAssertions;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions;
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
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
                PagePath.TeamMemberRoleInOrganisation,
            },
            CompaniesHouseSession = new ReExCompaniesHouseSession(),
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

    [TestMethod]
    public async Task AddApprovedPerson_CallsSaveSessionOnce()
    {      
        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull(); // No model passed to the view
        _sessionManagerMock.Verify(s => s.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
    }

    [TestMethod]
    public async Task AddApprovedPerson_ReturnsViewWithNoModel()
    {
        // Arrange
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeNull();
    }

    [TestMethod]
    public async Task AddApprovedPerson_ModelStateInvalid_ReturnsViewWithModel()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var model = new AddApprovedPersonViewModel();
        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);

        _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteAnotherApprovedPersonSelected_RedirectsToTeamMemberRole()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.IWillInviteAnotherApprovedPerson.ToString()
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteLaterSelected_ThrowsNotImplementedException()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.IWillInviteApprovedPersonLater.ToString()
        };

        // Act & Assert // need to re-visit once we found the correct URL
        await Assert.ThrowsExceptionAsync<NotImplementedException>(() =>
            _systemUnderTest.AddApprovedPerson(model)
        );
    }
}

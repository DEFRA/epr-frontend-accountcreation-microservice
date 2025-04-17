using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

/// <summary>
/// These tests should fail when the correct pages are plumbed in.
/// </summary>
[TestClass]
public class TeamMemberRoleInOrganisationTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
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
            CompaniesHouseSession = new CompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    [DataRow("with-invite", nameof(AccountCreationController.Declaration))]
    [DataRow("without-invite", nameof(AccountCreationController.CheckYourDetails))]
    [DataRow(null, nameof(AccountCreationController.Declaration))]
    public async Task TeamMemberRoleInOrganisation_RoleSavedAsDirector_Redirects_AndUpdateSession(string? invite, string actionName)
    {
        // Arrange
        var request = new TeamMemberRoleInOrganisationViewModel() { RoleInOrganisation = TeamMemberRoleInOrganisation.Director };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request, invite);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(actionName);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    [DataRow("with-invite", nameof(AccountCreationController.Declaration))]
    [DataRow("without-invite", nameof(AccountCreationController.CheckYourDetails))]
    [DataRow(null, nameof(AccountCreationController.Declaration))]
    public async Task TeamMemberRoleInOrganisation_RoleSavedAsCompanySecretary_RedirectsTo_AndUpdateSession(string? invite, string actionName)
    {
        // Arrange
        var request = new TeamMemberRoleInOrganisationViewModel() { RoleInOrganisation = TeamMemberRoleInOrganisation.CompanySecretary };

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request, invite);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(actionName);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_OrganisationRoleSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsConfirmCompanyDetails()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation(new TeamMemberRoleInOrganisationViewModel(), string.Empty);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        AssertBackLink(viewResult, "Pagebefore");
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisation_PageIsExited_BackLinkIsPageBefore()
    {
        //Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();
        AssertBackLink(viewResult, "Pagebefore");
    }
}

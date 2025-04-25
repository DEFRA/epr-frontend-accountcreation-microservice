using FluentAssertions;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.UnitTests;
using FrontendAccountCreation.Web.UnitTests.Controllers;
using FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;

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

    //todo: GET returns view, handles empty session, sets true/false in viewmodel, sets back link
    // POST: sets model, returns view when error, sets back link when error

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
    [DataRow(YesNoAnswer.Yes, true)]
    [DataRow(YesNoAnswer.No, false)]
    public async Task POST_UserSelectsYesOrNo_SessionUpdatedCorrectly(YesNoAnswer userAnswer, bool expectedIsTradingNameDifferentInSession)
    {
        // Arrange
        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = userAnswer };

        // Act
        await _systemUnderTest.IsTradingNameDifferent(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
            It.Is<OrganisationSession>(os => os.IsTradingNameDifferent == expectedIsTradingNameDifferentInSession)),
            Times.Once);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_SessionNotUpdated()
    {
        // Arrange
        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = null };
        _systemUnderTest.ModelState.AddModelError("IsTradingNameDifferent", "Select if your organisation's trading name is different to its Companies House name");

        // Act
        await _systemUnderTest.IsTradingNameDifferent(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(),
                It.IsAny<OrganisationSession>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_ViewIsReturnedWithCorrectModel()
    {
        // Arrange
        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = null };
        _systemUnderTest.ModelState.AddModelError("IsTradingNameDifferent", "Select if your organisation's trading name is different to its Companies House name");

        // Act
        var result = await _systemUnderTest.IsTradingNameDifferent(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<IsTradingNameDifferentViewModel>();
        var viewModel = (IsTradingNameDifferentViewModel?)viewResult.Model;
        viewModel!.IsTradingNameDifferent.Should().BeNull();
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes, nameof(OrganisationController.TradingName))]
    [DataRow(YesNoAnswer.No, nameof(OrganisationController.IsOrganisationAPartner))]
    public async Task POST_UserSelectsYesOrNo_UserIsRedirected(YesNoAnswer userAnswer, string expectedRedirect)
    {
        // Arrange
        var request = new IsTradingNameDifferentViewModel { IsTradingNameDifferent = userAnswer };

        // Act
        var result = await _systemUnderTest.IsTradingNameDifferent(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(expectedRedirect);
    }

    //[TestMethod]
    //[DataRow("with-invite", nameof(OrganisationController.ConfirmDetailsOfTheCompany))]
    //[DataRow("without-invite", nameof(OrganisationController.CompaniesHouseNumber))]
    //[DataRow(null, nameof(OrganisationController.ConfirmDetailsOfTheCompany))]
    //public async Task TeamMemberRoleInOrganisation_RoleSavedAsDirector_Redirects_AndUpdateSession(string? invite, string actionName)
    //{
    //    // Arrange
    //    var request = new TeamRoleViewModel { TeamRoleInOrganisation = TeamRoleInOrganisation.Director };

    //    // Act
    //    var result = await _systemUnderTest.TeamRole(request, invite);

    //    // Assert
    //    result.Should().BeOfType<RedirectToActionResult>();

    //    ((RedirectToActionResult)result).ActionName.Should().Be(actionName);

    //    _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    //}

    //[TestMethod]
    //[DataRow("with-invite", nameof(OrganisationController.ConfirmDetailsOfTheCompany))]
    //[DataRow("without-invite", nameof(OrganisationController.CompaniesHouseNumber))]
    //[DataRow(null, nameof(OrganisationController.ConfirmDetailsOfTheCompany))]
    //public async Task TeamMemberRoleInOrganisation_RoleSavedAsCompanySecretary_RedirectsTo_AndUpdateSession(string? invite, string actionName)
    //{
    //    // Arrange
    //    var request = new TeamMemberRoleInOrganisationViewModel() { RoleInOrganisation = ReExTeamMemberRole.CompanySecretary };

    //    // Act
    //    var result = await _systemUnderTest.TeamMemberRoleInOrganisation(request, invite);

    //    // Assert
    //    result.Should().BeOfType<RedirectToActionResult>();

    //    ((RedirectToActionResult)result).ActionName.Should().Be(actionName);

    //    _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    //}

    //[TestMethod]
    //public async Task TeamMemberRoleInOrganisation_OrganisationRoleSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsConfirmCompanyDetails()
    //{
    //    // Arrange
    //    _systemUnderTest.ModelState.AddModelError(nameof(TeamMemberRoleInOrganisationViewModel.RoleInOrganisation), "Field is required");

    //    // Act
    //    var result = await _systemUnderTest.TeamMemberRoleInOrganisation(new TeamMemberRoleInOrganisationViewModel(), string.Empty);

    //    // Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();

    //    _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    //    AssertBackLink(viewResult, "Pagebefore");
    //}

    //[TestMethod]
    //public async Task TeamMemberRoleInOrganisation_PageIsExited_BackLinkIsPageBefore()
    //{
    //    //Act
    //    var result = await _systemUnderTest.TeamMemberRoleInOrganisation();

    //    //Assert
    //    result.Should().BeOfType<ViewResult>();
    //    var viewResult = (ViewResult)result;
    //    viewResult.Model.Should().BeNull();
    //    AssertBackLink(viewResult, "Pagebefore");
    //}
}
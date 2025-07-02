using FluentAssertions;
using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Organisation;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class ManageControlOrganisationTests : OrganisationPageModelTestBase<ManageControlOrganisation>
{
    private ManageControlOrganisation _manageControlOrganisation;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        OrganisationSession.Journey =
        [
            PagePath.ManageControlOrganisation,
            PagePath.TeamMemberDetails
        ];
        OrganisationSession.ReExCompaniesHouseSession = new ReExCompaniesHouseSession();
        OrganisationSession.IsUserChangingDetails = false;

        _manageControlOrganisation = new ManageControlOrganisation(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
        {
            PageContext = PageContext
        };
    }

    [TestMethod]
    [DataRow(YesNoNotSure.Yes)]
    [DataRow(YesNoNotSure.No)]
    [DataRow(YesNoNotSure.NotSure)]
    public async Task OnGet_SetsSelectedValueCorrectly(YesNoNotSure? yesNoNotSure)
    {
        // Arrange
        OrganisationSession.TheyManageOrControlOrganisation = yesNoNotSure;

        // Act
        await _manageControlOrganisation.OnGet();

        // Assert
        _manageControlOrganisation.SelectedValue.Should().Be(yesNoNotSure.ToString());
    }

    [TestMethod]
    public async Task OnGet_TheyManageOrControlOrganisationIsNull_SetsSelectedValueCorrectly()
    {
        // Act
        await _manageControlOrganisation.OnGet();

        // Assert
        _manageControlOrganisation.SelectedValue.Should().BeNull();
    }

    [TestMethod]
    [DataRow(YesNoNotSure.Yes)]
    [DataRow(null)]
    public async Task OnGet_InvitePerson_SetsSelectedValueCorrectly(YesNoNotSure? yesNoNotSure)
    {
        // Arrange
        OrganisationSession.TheyManageOrControlOrganisation = yesNoNotSure;

        var invitePerson = yesNoNotSure == YesNoNotSure.Yes;

        // Act
        await _manageControlOrganisation.OnGet(invitePerson);

        // Assert
        _manageControlOrganisation.SelectedValue.Should().BeNull();
    }

    [TestMethod]
    public async Task OnPost_InvalidModelState_ReturnsPageWithCorrectErrors()
    {
        // Arrange
        _manageControlOrganisation.ModelState.AddModelError("TheyManageOrControlOrganisation", "Required");

        // Act
        await _manageControlOrganisation.OnPost();

        // Assert
        _manageControlOrganisation.Errors.HasErrors.Should().BeTrue();
        _manageControlOrganisation.Errors.Should().NotBeNull();
        _manageControlOrganisation.Errors.Errors.Should().ContainSingle(e => e.HtmlElementId == "TheyManageOrControlOrganisation");
        _manageControlOrganisation.Errors.Errors.Should().ContainSingle(e => e.Message == "Required");
    }

    //todo: split this into 2?
    [TestMethod]
    [DataRow(YesNoNotSure.Yes, nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails), PagePath.NonCompaniesHouseTeamMemberDetails)]
    [DataRow(YesNoNotSure.No, nameof(ApprovedPersonController.PersonCanNotBeInvited), PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
    [DataRow(YesNoNotSure.NotSure, nameof(ApprovedPersonController.PersonCanNotBeInvited), PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
    public async Task OnPost_ValidModel_Redirects(
        YesNoNotSure yesNoNotSure, string expectedActionName, string expectedNextPagePath)
    {
        // Arrange
        _manageControlOrganisation.SelectedValue = yesNoNotSure.ToString();

        // Act
        var result = await _manageControlOrganisation.OnPost();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(expectedActionName);
        redirectResult.RouteValues?["currentPagePath"].Should().Be(PagePath.ManageControlOrganisation);
        redirectResult.RouteValues?["nextPagePath"].Should().Be(expectedNextPagePath);
    }

    //todo: more tests for checking session saved etc.
}
using FluentAssertions;
using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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
            PagePath.AddAnApprovedPerson,
            PagePath.ManageControlOrganisation
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
    public async Task OnGet_BackLinkIsSetCorrectly()
    {
        // Act
        await _manageControlOrganisation.OnGet();

        // Assert
        AssertBackLink(_manageControlOrganisation, PagePath.AddAnApprovedPerson);
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

    //to-do: split this into 2?
    [TestMethod]
    [DataRow(YesNoNotSure.Yes, nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails), PagePath.NonCompaniesHouseTeamMemberDetails)]
    [DataRow(YesNoNotSure.No, nameof(ApprovedPersonController.PersonCanNotBeInvited), PagePath.ApprovedPersonCanNotBeInvited)]
    [DataRow(YesNoNotSure.NotSure, nameof(ApprovedPersonController.PersonCanNotBeInvited), PagePath.ApprovedPersonCanNotBeInvited)]
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

    [TestMethod]
    public async Task OnPost_InvalidModelState_BackLinkIsSetCorrectly()
    {
        // Arrange
        _manageControlOrganisation.ModelState.AddModelError("TheyManageOrControlOrganisation", "Required");

        // Act
        await _manageControlOrganisation.OnPost();

        // Assert
        AssertBackLink(_manageControlOrganisation, PagePath.AddAnApprovedPerson);
    }

    //to-do: more tests for checking session saved etc.

    [TestMethod]
    public void Radios_ShouldReturnYesNoNotSureRadios()
    {
        // Act
        var radios = _manageControlOrganisation.Radios.ToList();

        // Assert
        radios.Should().HaveCount(3);
        radios[0].Value.Should().Be("Yes");
        radios[1].Value.Should().Be("No");
        radios[2].Value.Should().Be("NotSure");
        //to-do: setup shared localizer and check localized strings
    }

    [TestMethod]
    public void Question_ShouldReturnLocalizedQuestion()
    {
        // Arrange
        LocalizerMock.Setup(l => l["ManageControlOrganisation.Question"])
            .Returns(new LocalizedString("ManageControlOrganisation.Question", "Test question string"));

        // Act
        var question = _manageControlOrganisation.Question;

        // Assert
        question.Should().Be("Test question string");
    }

    [TestMethod]
    public void Hint_ShouldReturnLocalizedDescription()
    {
        // Arrange
        LocalizerMock.Setup(l => l["ManageControlOrganisation.Hint"])
            .Returns(new LocalizedString("ManageControlOrganisation.Hint", "Test hint string"));

        // Act
        var hint = _manageControlOrganisation.Hint;

        // Assert
        hint.Should().Be("Test hint string");
    }
}
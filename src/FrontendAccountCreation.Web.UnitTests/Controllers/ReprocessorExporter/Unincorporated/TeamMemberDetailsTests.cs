using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Unincorporated;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Unincorporated;

[TestClass]
public class TeamMemberDetailsTests : UnincorporatedTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey = new List<string> {
                PagePath.UnincorporatedManageControlOrganisation,
                PagePath.UnincorporatedTeamMemberDetails
            },
            ReExUnincorporatedFlowSession = new ReExUnincorporatedFlowSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task Get_ReturnsViewWithPrepopulatedViewModel()
    {
        // Arrange
        Guid? expectedGuidInFocus = null;

        

        var teamMemberDetails = new ReExTeamMemberDetails
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test 2",
            Email = "test@test.test",
            Telephone = "00000000000"
        };

        var teamMemberDetailsDictionary = new Dictionary<Guid, ReExTeamMemberDetails>
        {
            { teamMemberDetails.Id, teamMemberDetails },
        };

        _tempDataDictionaryMock.Setup(t => t["FocusId"]).Returns(teamMemberDetails.Id.ToString());
        _tempDataDictionaryMock.SetupSet(t => t["FocusId"] = It.IsAny<object>())
           .Callback((string key, object value) => expectedGuidInFocus = (Guid)value);

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary = teamMemberDetailsDictionary;
        
        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        AssertBackLink(viewResult, PagePath.UnincorporatedManageControlOrganisation);

        var viewModel = viewResult.Model.Should().BeOfType<ReExTeamMemberDetailsViewModel>().Subject;
        viewModel.Id.Should().Be(teamMemberDetails.Id);
        viewModel.FirstName.Should().Be(teamMemberDetails.FirstName);
        viewModel.LastName.Should().Be(teamMemberDetails.LastName);
        viewModel.Email.Should().Be(teamMemberDetails.Email);
        viewModel.Telephone.Should().Be(teamMemberDetails.Telephone);

        expectedGuidInFocus.Should().NotBeNull();
        expectedGuidInFocus.Should().Be(teamMemberDetails.Id);
    }

    [TestMethod]
    public async Task Get_ReturnsViewWithEmptyViewModel_WhenFocusIdNotSpecified()
    {
        // Arrange
        var teamMemberDetails = new ReExTeamMemberDetails
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test 2",
            Email = "test@test.test",
            Telephone = "00000000000"
        };

        var teamMemberDetailsDictionary = new Dictionary<Guid, ReExTeamMemberDetails>
        {
            { teamMemberDetails.Id, teamMemberDetails },
        };

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary = teamMemberDetailsDictionary;

        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        AssertBackLink(viewResult, PagePath.UnincorporatedManageControlOrganisation);

        var viewModel = viewResult.Model.Should().BeOfType<ReExTeamMemberDetailsViewModel>().Subject;
        viewModel.Id.Should().BeNull();
        viewModel.FirstName.Should().BeNull();
        viewModel.LastName.Should().BeNull();
        viewModel.Email.Should().BeNull();
        viewModel.Telephone.Should().BeNull();
    }

    [TestMethod]
    public async Task Get_ReturnsViewWithEmptyViewModel_WhenDictionaryDoesNotExist()
    {
        // Arrange
        _tempDataDictionaryMock.Setup(t => t["FocusId"]).Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        AssertBackLink(viewResult, PagePath.UnincorporatedManageControlOrganisation);

        var viewModel = viewResult.Model.Should().BeOfType<ReExTeamMemberDetailsViewModel>().Subject;
        viewModel.Id.Should().BeNull();
        viewModel.FirstName.Should().BeNull();
        viewModel.LastName.Should().BeNull();
        viewModel.Email.Should().BeNull();
        viewModel.Telephone.Should().BeNull();
    }

    [TestMethod]
    public async Task Get_ReturnsViewWithEmptyViewModel_WhenFocusIdIsNotInDictionary()
    {
        // Arrange
        var teamMemberDetails = new ReExTeamMemberDetails
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test 2",
            Email = "test@test.test",
            Telephone = "00000000000"
        };

        var teamMemberDetailsDictionary = new Dictionary<Guid, ReExTeamMemberDetails>
        {
            { teamMemberDetails.Id, teamMemberDetails },
        };

        _tempDataDictionaryMock.Setup(t => t["FocusId"]).Returns(Guid.Empty.ToString());
        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary = teamMemberDetailsDictionary;

        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        AssertBackLink(viewResult, PagePath.UnincorporatedManageControlOrganisation);

        var viewModel = viewResult.Model.Should().BeOfType<ReExTeamMemberDetailsViewModel>().Subject;
        viewModel.Id.Should().BeNull();
        viewModel.FirstName.Should().BeNull();
        viewModel.LastName.Should().BeNull();
        viewModel.Email.Should().BeNull();
        viewModel.Telephone.Should().BeNull();
    }

    [TestMethod]
    public async Task Post_ReturnsViews_WhenModelIsInvalid()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError("Role", "Test");

        var viewModel = new ReExTeamMemberDetailsViewModel();
        
        // Act
        var result = await _systemUnderTest.TeamMemberDetails(viewModel);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        AssertBackLink(viewResult, PagePath.UnincorporatedManageControlOrganisation);
        viewResult.Model.Should().Be(viewModel);
    }

    [TestMethod]
    public async Task Post_CreatesTeamDetailsSession_AndSavesTeamDetails_AndRedirectsToCheckInvitation_WhenTeamDetailsSessionDoesNotExist()
    {
        // Arrange
        Guid? expectedGuidInFocus = null;

        var viewModel = new ReExTeamMemberDetailsViewModel
        {
            FirstName = "Test",
            LastName = "Test 2",
            Email = "test@test.test",
            Telephone = "00000000000"
        };

        _tempDataDictionaryMock.SetupSet(t => t["FocusId"] = It.IsAny<object>())
           .Callback((string key, object value) => expectedGuidInFocus = (Guid)value);

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.CheckInvitation));

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary.Count.Should().Be(1);
        var teamMemberDetails = _organisationSession.ReExUnincorporatedFlowSession
            .TeamMemberDetailsDictionary
            .Values
            .First();

        teamMemberDetails.FirstName.Should().Be(viewModel.FirstName);
        teamMemberDetails.LastName.Should().Be(viewModel.LastName);
        teamMemberDetails.Email.Should().Be(viewModel.Email);
        teamMemberDetails.Telephone.Should().Be(viewModel.Telephone);

        expectedGuidInFocus.Should().NotBeNull();
        expectedGuidInFocus.Should().Be(teamMemberDetails.Id);
    }

    [TestMethod]
    public async Task Post_SavesTeamDetails_AndRedirectsToCheckInvitation_WhenTeamDetailsSessionExist()
    {
        // Arrange
        Guid? expectedGuidInFocus = null;

        var viewModel = new ReExTeamMemberDetailsViewModel
        {
            FirstName = "Test",
            LastName = "Test 2",
            Email = "test@test.test",
            Telephone = "00000000000"
        };

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary = new Dictionary<Guid, ReExTeamMemberDetails>();

        _tempDataDictionaryMock.SetupSet(t => t["FocusId"] = It.IsAny<object>())
           .Callback((string key, object value) => expectedGuidInFocus = (Guid)value);

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.CheckInvitation));

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary.Count.Should().Be(1);
        var teamMemberDetails = _organisationSession.ReExUnincorporatedFlowSession
            .TeamMemberDetailsDictionary
            .Values
            .First();

        teamMemberDetails.FirstName.Should().Be(viewModel.FirstName);
        teamMemberDetails.LastName.Should().Be(viewModel.LastName);
        teamMemberDetails.Email.Should().Be(viewModel.Email);
        teamMemberDetails.Telephone.Should().Be(viewModel.Telephone);

        expectedGuidInFocus.Should().NotBeNull();
        expectedGuidInFocus.Should().Be(teamMemberDetails.Id);
    }

    [TestMethod]
    public async Task Post_SavesNewTeamDetails_AndRedirectsToCheckInvitation_WhenTeamDetailsIdNotFoundInTheSession()
    {
        // Arrange
        Guid? expectedGuidInFocus = null;
        var oldId = Guid.NewGuid();

        var viewModel = new ReExTeamMemberDetailsViewModel
        {
            Id = oldId,
            FirstName = "Test",
            LastName = "Test 2",
            Email = "test@test.test",
            Telephone = "00000000000"
        };

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary = new Dictionary<Guid, ReExTeamMemberDetails>
        {
            { Guid.NewGuid(), new ReExTeamMemberDetails() }
        };

        _tempDataDictionaryMock.SetupSet(t => t["FocusId"] = It.IsAny<object>())
           .Callback((string key, object value) => expectedGuidInFocus = (Guid)value);

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.CheckInvitation));

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary.Count.Should().Be(2);
        var teamMemberDetails = _organisationSession.ReExUnincorporatedFlowSession
            .TeamMemberDetailsDictionary
            .Values
            .Last();

        teamMemberDetails.Id.Should().NotBe(oldId);
        teamMemberDetails.FirstName.Should().Be(viewModel.FirstName);
        teamMemberDetails.LastName.Should().Be(viewModel.LastName);
        teamMemberDetails.Email.Should().Be(viewModel.Email);
        teamMemberDetails.Telephone.Should().Be(viewModel.Telephone);

        expectedGuidInFocus.Should().NotBeNull();
        expectedGuidInFocus.Should().Be(teamMemberDetails.Id);
    }

    [TestMethod]
    public async Task Post_UpdatesDetailsInSession_AndRedirectsToCheckInvitationAction_WhenSessionExist_AndHasTheDetails()
    {
        // Arrange
        Guid? expectedGuidInFocus = null;
        var inSessionTeamMemberDetails = new ReExTeamMemberDetails
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test 2",
            Email = "test@test.test",
            Telephone = "00000000000"
        };

        var teamMemberDetailsDictionary = new Dictionary<Guid, ReExTeamMemberDetails>
        {
            { inSessionTeamMemberDetails.Id, inSessionTeamMemberDetails },
        };

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary = teamMemberDetailsDictionary;

        _tempDataDictionaryMock.SetupSet(t => t["FocusId"] = It.IsAny<object>())
           .Callback((string key, object value) => expectedGuidInFocus = (Guid)value);

        var viewModel = new ReExTeamMemberDetailsViewModel
        {
            Id = inSessionTeamMemberDetails.Id,
            FirstName = "Test 2",
            LastName = "Test 3",
            Email = "test_2@test.test",
            Telephone = "00000000001"
        };

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.CheckInvitation));

        _organisationSession.ReExUnincorporatedFlowSession.TeamMemberDetailsDictionary.Count.Should().Be(1);

        inSessionTeamMemberDetails.FirstName.Should().Be(viewModel.FirstName);
        inSessionTeamMemberDetails.LastName.Should().Be(viewModel.LastName);
        inSessionTeamMemberDetails.Email.Should().Be(viewModel.Email);
        inSessionTeamMemberDetails.Telephone.Should().Be(viewModel.Telephone);

        expectedGuidInFocus.Should().NotBeNull();
        expectedGuidInFocus.Should().Be(inSessionTeamMemberDetails.Id);
    }
}
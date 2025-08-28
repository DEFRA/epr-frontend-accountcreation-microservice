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
public class TeamMembersDetailsTests : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock = null!;
    private readonly Guid _teamMemberId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey =
            [
                "PageBefore", //Todo: replace it when the page is implemented
                PagePath.TeamMemberRoleInOrganisation
            ],
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers =
                [
                    new ReExCompanyTeamMember
                    {
                        Id = _teamMemberId,
                        FirstName = "John",
                        LastName = "Smith",
                        TelephoneNumber = "0123456789",
                        Email = "john@example.com"
                    }
                ]
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_WithExistingTeamMember_ReturnsPopulatedView()
    {
        // Arrange
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(_teamMemberId);

        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<TeamMemberViewModel>().Subject;

        model.FirstName.Should().Be("John");
        model.LastName.Should().Be("Smith");
        model.Telephone.Should().Be("0123456789");
        model.Email.Should().Be("john@example.com");
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_WithInvalidId_ReturnsEmptyView()
    {
        // Arrange
        var invalidId = Guid.NewGuid(); // doesn't match any member
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(invalidId);

        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
    }

    [TestMethod]
    public async Task TeamMemberDetails_Post_WithInvalidModel_ReturnsViewWithSameModel()
    {
        // Arrange
        var model = new TeamMemberViewModel
        {
            Id = _teamMemberId,
            FirstName = "Jane",
            LastName = "Doe"
            // Missing Email and Telephone
        };

        _systemUnderTest.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeEquivalentTo(model);
    }

    [TestMethod]
    public async Task TeamMemberDetails_Post_WithValidModel_UpdatesAndRedirects()
    {
        // Arrange
        var model = new TeamMemberViewModel
        {
            Id = _teamMemberId,
            FirstName = "Jane",
            LastName = "Doe",
            Telephone = "0987654321",
            Email = "jane@example.com"
        };

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var updatedMember = _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.First(x => x.Id == _teamMemberId);
        updatedMember.FirstName.Should().Be("Jane");
        updatedMember.LastName.Should().Be("Doe");
        updatedMember.TelephoneNumber.Should().Be("0987654321");
        updatedMember.Email.Should().Be("jane@example.com");

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_WhenTeamMembersIsNull_ReturnsEmptyView()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = null;
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(_teamMemberId);

        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_WhenReExCompaniesHouseSessionIsNull_ReturnsEmptyView()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession = null;
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(_teamMemberId);

        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_CallsSaveSession()
    {
        // Arrange
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(_teamMemberId);

        // Act
        _ = await _systemUnderTest.TeamMemberDetails();

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberDetails_Post_WhenTeamMembersIsNull_DoesNotThrow()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = null;

        var model = new TeamMemberViewModel
        {
            Id = _teamMemberId,
            FirstName = "Updated",
            LastName = "Updated",
            Telephone = "123456",
            Email = "update@example.com"
        };

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMembersCheckInvitationDetails));
    }

    [TestMethod]
    public async Task TeamMemberDetails_Post_WhenTeamMemberNotFound_DoesNotUpdateOrThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var model = new TeamMemberViewModel
        {
            Id = nonExistentId,
            FirstName = "Ghost",
            LastName = "Ghost",
            Telephone = "0000",
            Email = "ghost@example.com"
        };

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMembersCheckInvitationDetails));

        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers
            .Exists(x => x.LastName == "Ghost").Should().BeFalse();
    }

    [TestMethod]
    public async Task TeamMemberDetailsEdit_Get_RedirectsTo_TeamMemberDetails()
    {
        var result = await _systemUnderTest.TeamMemberDetailsEdit(Guid.NewGuid());

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberDetails));
    }
}
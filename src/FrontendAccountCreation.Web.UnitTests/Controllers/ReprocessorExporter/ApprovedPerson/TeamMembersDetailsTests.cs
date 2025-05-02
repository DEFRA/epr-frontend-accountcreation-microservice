using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
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
                "PageBefore",
                PagePath.TeamMemberRoleInOrganisation
            ],
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers =
                [
                    new ReExCompanyTeamMember
                    {
                        Id = _teamMemberId,
                        FullName = "John Smith",
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
        // Act
        var result = await _systemUnderTest.TeamMemberDetails(_teamMemberId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<TeamMemberViewModel>().Subject;

        model.FullName.Should().Be("John Smith");
        model.Telephone.Should().Be("0123456789");
        model.Email.Should().Be("john@example.com");
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_WithInvalidId_ReturnsEmptyView()
    {
        // Arrange
        var invalidId = Guid.NewGuid(); // doesn't match any member

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(invalidId);

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
            FullName = "Jane Doe"
            // Missing Email and Telephone
        };

        _systemUnderTest.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeEquivalentTo(model);
        AssertBackLink(viewResult, PagePath.TeamMemberDetails);
    }

    [TestMethod]
    public async Task TeamMemberDetails_Post_WithValidModel_UpdatesAndRedirects()
    {
        // Arrange
        var model = new TeamMemberViewModel
        {
            Id = _teamMemberId,
            FullName = "Jane Doe",
            Telephone = "0987654321",
            Email = "jane@example.com"
        };

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var updatedMember = _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.First(x => x.Id == _teamMemberId);
        updatedMember.FullName.Should().Be("Jane Doe");
        updatedMember.TelephoneNumber.Should().Be("0987654321");
        updatedMember.Email.Should().Be("jane@example.com");

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once);
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_WhenTeamMembersIsNull_ReturnsEmptyView()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = null;

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(_teamMemberId);

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

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(_teamMemberId);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
    }

    [TestMethod]
    public async Task TeamMemberDetails_Get_CallsSaveSession()
    {
        // Act
        var result = await _systemUnderTest.TeamMemberDetails(_teamMemberId);

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
            FullName = "Updated",
            Telephone = "123456",
            Email = "update@example.com"
        };

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        // No update should happen but also no exception
    }

    [TestMethod]
    public async Task TeamMemberDetails_Post_WhenTeamMemberNotFound_DoesNotUpdateOrThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var model = new TeamMemberViewModel
        {
            Id = nonExistentId,
            FullName = "Ghost",
            Telephone = "0000",
            Email = "ghost@example.com"
        };

        // Act
        var result = await _systemUnderTest.TeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers
            .Any(x => x.FullName == "Ghost").Should().BeFalse();
    }

}
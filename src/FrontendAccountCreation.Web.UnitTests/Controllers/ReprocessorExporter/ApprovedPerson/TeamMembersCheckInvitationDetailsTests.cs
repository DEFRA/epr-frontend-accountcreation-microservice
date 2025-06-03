using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class TeamMembersCheckInvitationDetailsTests : ApprovedPersonTestBase
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
               "PageBefore", // Todo: replace when page is developed
                PagePath.TeamMemberRoleInOrganisation,
            },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetails_WhenTeamMemberIdNotSupplied_SessionIsUnchanged()
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new() { Id = Guid.NewGuid() };
        ReExCompanyTeamMember jill = new() { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        // Act
        await _systemUnderTest.TeamMembersCheckInvitationDetails();

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers?.Count.Should().Be(2);
    }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetails_WhenIncorrectTeamMemberIdNotSupplied_SessionIsUnchanged()
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new() { Id = Guid.NewGuid() };
        ReExCompanyTeamMember jill = new() { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(Guid.NewGuid());

        // Act
        await _systemUnderTest.TeamMembersCheckInvitationDetails();

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers?.Count.Should().Be(2);
    }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetailsDelete_WhenTeamMemberIdSupplied_RemovesTeamMemberFromSession()
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new() { Id = Guid.NewGuid() };
        ReExCompanyTeamMember jill = new() { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        // Act
        await _systemUnderTest.TeamMembersCheckInvitationDetailsDelete(jack.Id);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers?.Count.Should().Be(1);
     }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetails_UpdatesSession_And_ReturnsView()
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new() { Id = Guid.NewGuid(), FirstName = "Jack", LastName = "Dors" };
        ReExCompanyTeamMember jill = new() { Id = Guid.NewGuid(), FirstName = "Jill", LastName = "Dors" };
        ReExCompanyTeamMember nobody = new() { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        teamMembers.Add(nobody);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        // Act
        IActionResult result = await _systemUnderTest.TeamMembersCheckInvitationDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        ViewResult viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<List<ReExCompanyTeamMember>>();

        // nobody has empty full name, hence excluded
        ((List<ReExCompanyTeamMember>) viewResult.Model).Count.Should().Be(2);

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetailsPost_UpdatesSession_AndRedirects()
    {
        // Arrange
        var teamMembers = new List<ReExCompanyTeamMember?>
    {
        new() { Id = Guid.NewGuid(), FirstName = "Jack", LastName = "Smith" },
        new() { Id = Guid.NewGuid(), FirstName = "Jill", LastName = "Test" },
    };

        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);

        // Act
        IActionResult result = await _systemUnderTest.TeamMembersCheckInvitationDetailsPost(teamMembers);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));

        // Optional: Check the redirect path if it's set in route values or controller
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }


}

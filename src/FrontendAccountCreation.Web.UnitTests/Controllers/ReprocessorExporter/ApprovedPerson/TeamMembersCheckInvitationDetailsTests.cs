using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

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
               "Pagebefore", // replace when page is developed
                PagePath.TeamMemberRoleInOrganisation,
            },
            CompaniesHouseSession = new ReExCompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("ee876b41-0f6b-481f-b574-4b4df313323b")]
    public async Task TeamMembersCheckInvitationDetails_WhenIncorrectTeamMemberIdNotSupplied_SessionIsUnchanged(Guid? id)
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new ReExCompanyTeamMember { Id = Guid.NewGuid() };
        ReExCompanyTeamMember jill = new ReExCompanyTeamMember { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        _orgSessionMock.CompaniesHouseSession.TeamMembers = teamMembers;

        // Act
        await _systemUnderTest.TeamMembersCheckInvitationDetails(id);

        // Assert
        _orgSessionMock.CompaniesHouseSession.TeamMembers?.Count().Should().Be(2);
    }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetails_WhenTeamMemberIdSupplied_RemovesTeamMemberFromSession()
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new ReExCompanyTeamMember { Id = Guid.NewGuid() };
        ReExCompanyTeamMember jill = new ReExCompanyTeamMember { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        _orgSessionMock.CompaniesHouseSession.TeamMembers = teamMembers;
        
        // Act
        await _systemUnderTest.TeamMembersCheckInvitationDetails(jack.Id);

        // Assert
        _orgSessionMock.CompaniesHouseSession.TeamMembers?.Count().Should().Be(1);
     }
}

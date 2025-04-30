using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                ReExPagePath.TeamMemberRoleInOrganisation,
            },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession(),
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
        ReExCompanyTeamMember jack = new() { Id = Guid.NewGuid() };
        ReExCompanyTeamMember jill = new() { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        // Act
        await _systemUnderTest.TeamMembersCheckInvitationDetails(id);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers?.Count().Should().Be(2);
    }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetails_WhenTeamMemberIdSupplied_RemovesTeamMemberFromSession()
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new() { Id = Guid.NewGuid() };
        ReExCompanyTeamMember jill = new() { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;
        
        // Act
        await _systemUnderTest.TeamMembersCheckInvitationDetails(jack.Id);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers?.Count().Should().Be(1);
     }

    [TestMethod]
    public async Task TeamMembersCheckInvitationDetails_UpdatesSession_And_ReturnsView()
    {
        // Arrange
        List<ReExCompanyTeamMember?> teamMembers = [];
        ReExCompanyTeamMember jack = new() { Id = Guid.NewGuid(), FullName = "Jack Dors" };
        ReExCompanyTeamMember jill = new() { Id = Guid.NewGuid(), FullName = "Jill Dors" };
        ReExCompanyTeamMember nobby = new() { Id = Guid.NewGuid() };
        teamMembers.Add(jack);
        teamMembers.Add(jill);
        teamMembers.Add(nobby);
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers = teamMembers;

        // Act
        IActionResult result = await _systemUnderTest.TeamMembersCheckInvitationDetails(jack.Id);

        // Assert
        result.Should().BeOfType<List<ReExCompanyTeamMember>>();

        ViewResult viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<TeamMemberRoleInOrganisationViewModel>();

        // nobby has empty full name, hence excluded from view reult
        ((List<ReExCompanyTeamMember>) viewResult.Model).Count.Should().Be(2);

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
        AssertBackLink(viewResult, "Pagebefore");
    }
}

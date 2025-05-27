using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class YouAreApprovedPersonTests : ApprovedPersonTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_YouAreApprovedPerson_Returns_View()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task ContinueCalls_CheckYourDetails_And_Redirects_ToDesired_View()
    {
        // Arrange
        var orgSessionMock = new OrganisationSession
        {
            Journey = [PagePath.ApprovedPersonContinue]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgSessionMock)
            .Verifiable();

        // Act
        var result = _systemUnderTest.CheckYourDetails();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskStatus.RanToCompletion);
    }

    // TO DO following & modify - once Tungsten has merged
    [TestMethod]
    [DataRow(null, null)]
    [DataRow("acfa4773-20f0-4cf4-ae03-b36d96a8589a", "acfa4773-20f0-4cf4-ae03-b36d96a8589a")]
    public async Task InviteLink_Calls_TeamMemberRoleInOrganisation_And_Redirects_ToDesired_View(string? id, string teamMemberId)
    {
        // Arrange
        Guid memberId = !string.IsNullOrWhiteSpace(teamMemberId) ? Guid.Parse(teamMemberId) : Guid.Empty;
        var orgSessionMock = new OrganisationSession
        {
            Journey = [PagePath.AddApprovedPerson],
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers =
                [
                   new() { Id = memberId, Role = ReExTeamMemberRole.Director, FullName = "test", Email = "test@test.com", TelephoneNumber = "07880908087" }
                ]
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgSessionMock)
            .Verifiable();

        Guid? queryId = !string.IsNullOrWhiteSpace(id) ? Guid.Parse(id) : null;

        // Act
        var result = _systemUnderTest.TeamMemberRoleInOrganisation(queryId);

        // Assert
        result.Should().NotBeNull();
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task Get_YouAreApprovedPerson_WhenOrganisationIsPartnership_Returns_LimitedPartnershipView()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("LimitedPartnershipYouAreApprovedPerson");
    }

    [TestMethod]
    public async Task Get_YouAreApprovedPerson_WhenOrganisationIsNotPartnership_Returns_DefaultView()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().BeNull(); // default view
    }

    [TestMethod]
    public async Task Post_YouAreApprovedPerson_InviteApprovedPersonTrue_RedirectsToTeamMemberRole()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson(inviteApprovedPerson: true);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task Post_YouAreApprovedPerson_InviteApprovedPersonFalse_RedirectsToCheckYourDetails()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson(inviteApprovedPerson: false);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));
    }
}

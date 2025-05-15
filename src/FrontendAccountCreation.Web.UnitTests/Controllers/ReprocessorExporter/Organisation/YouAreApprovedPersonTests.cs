using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using global::FrontendAccountCreation.Web.Constants;
using global::FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

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
        var okResult = (Microsoft.AspNetCore.Mvc.OkObjectResult)result.Result;
        
        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskStatus.RanToCompletion);
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    // TO DO following & modify - once Tungsten has merged
    [TestMethod]
    [DataRow (null, null)]
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

        Guid? queryId = !string.IsNullOrWhiteSpace(id) ? Guid.Parse(id) : (Guid?)null;

        // Act
        var result = _systemUnderTest.TeamMemberRoleInOrganisation(queryId);

        // Assert
        result.Should().NotBeNull();
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }
}

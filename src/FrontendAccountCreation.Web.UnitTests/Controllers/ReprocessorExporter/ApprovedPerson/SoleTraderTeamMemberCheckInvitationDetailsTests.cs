using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class SoleTraderTeamMemberCheckInvitationDetailsTests : ApprovedPersonTestBase
{
    private OrganisationSession? _orgSessionMock;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.SoleTraderTeamMemberDetails,
                PagePath.SoleTraderTeamMemberCheckInvitationDetails,
                PagePath.CheckYourDetails,
                PagePath.SoleTraderTeamMemberCheckInvitationDetailsDelete
            ],
            ReExManualInputSession = new ReExManualInputSession(),
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task SoleTraderTeamMemberCheckInvitationDetailsDelete_WhenTeamMemberExists_RemovesTeamMember_AndRedirects()
    {
        //Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMember = new ReExCompanyTeamMember
                {
                    FirstName = "Teddy",
                    LastName = "Drowns",
                    Email = "teammember@example.com",
                    TelephoneNumber = "01234567890"
                }
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberCheckInvitationDetailsDelete();

        // Assert
        session.ReExManualInputSession.TeamMember.Should().BeNull();

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.SoleTraderTeamMemberCheckInvitationDetails));
    }

    [TestMethod]
    public async Task SoleTraderTeamMemberCheckInvitationDetails_WhenSessionHasTeamMember_ReturnsViewWithModel()
    {
        // Arrange
        var teamMember = new ReExCompanyTeamMember
        {
            FirstName = "John",
            LastName = "Smith",
            Email = "teammember@email.com",
            TelephoneNumber = "01234567890"
        };

        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMember = teamMember
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberCheckInvitationDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeEquivalentTo(teamMember);
    }

    [TestMethod]
    public async Task SoleTraderTeamMemberCheckInvitationDetailsPost_WhenCalled_RedirectsToCheckYourDetails()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMember = new ReExCompanyTeamMember
                {
                    FirstName = "John",
                    LastName = "Smith"
                }
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberCheckInvitationDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));
    }

    [TestMethod]
    public async Task SoleTraderTeamMemberCheckInvitationDetails_WhenNoTeamMember_ReturnsViewWithNullModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMember = null
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberCheckInvitationDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeNull();
    }
}
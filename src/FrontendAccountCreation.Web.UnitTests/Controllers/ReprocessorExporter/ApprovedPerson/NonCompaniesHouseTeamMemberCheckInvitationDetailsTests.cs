using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class NonCompaniesHouseTeamMemberCheckInvitationDetailsTests : ApprovedPersonTestBase
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
                PagePath.NonCompaniesHouseTeamMemberDetails,
                PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails,
                PagePath.CheckYourDetails,
                PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetailsDelete
            ],
            ReExManualInputSession = new ReExManualInputSession(),
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberCheckInvitationDetailsDelete_WhenTeamMemberExists_RemovesTeamMember_AndRedirects()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var teamMember = new ReExCompanyTeamMember
        {
            Id = memberId,
            FirstName = "Teddy",
            LastName = "Drowns",
            Email = "teammember@example.com",
            TelephoneNumber = "01234567890"
        };

        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember> { teamMember }
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberCheckInvitationDetailsDelete(memberId);

        // Assert
        session.ReExManualInputSession.TeamMembers.Should().NotContain(m => m.Id == memberId);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.NonCompaniesHouseTeamMemberCheckInvitationDetails));
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberCheckInvitationDetails_WhenSessionHasTeamMember_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var teamMember = new ReExCompanyTeamMember
        {
            Id = expectedId,
            FirstName = "John",
            LastName = "Smith",
            Email = "teammember@email.com",
            TelephoneNumber = "01234567890"
        };

        var session = new OrganisationSession
        {
            IsUkMainAddress = false,
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.SoleTrader,
                TeamMembers = new List<ReExCompanyTeamMember> { teamMember }
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberCheckInvitationDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;

        viewResult.Model.Should().BeOfType<NonCompaniesHouseTeamMemberCheckInvitationDetailsViewModel>();
        var model = viewResult.Model as NonCompaniesHouseTeamMemberCheckInvitationDetailsViewModel;

        model.TeamMembers.Should().NotBeNull();
        model.TeamMembers.Should().HaveCount(1);
        model.TeamMembers[0].Should().BeEquivalentTo(teamMember);

        model.IsNonUk.Should().BeTrue();
        model.IsSoleTrader.Should().BeTrue();
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberCheckInvitationDetailsPost_WhenCalled_RedirectsToCheckYourDetails()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
                {
                    new ReExCompanyTeamMember
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "John",
                        LastName = "Smith",
                        Email = "teammember@email.com",
                        TelephoneNumber = "01234567890"
                    }
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
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberCheckInvitationDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberCheckInvitationDetails_WhenNoTeamMember_ReturnsViewWithModelAndNullTeamMembers()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsUkMainAddress = true,
            ReExManualInputSession = new ReExManualInputSession()
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberCheckInvitationDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<NonCompaniesHouseTeamMemberCheckInvitationDetailsViewModel>();
        var model = (NonCompaniesHouseTeamMemberCheckInvitationDetailsViewModel)viewResult.Model;

        model.TeamMembers.Should().BeNull();
        model.IsSoleTrader.Should().BeFalse();
        model.IsNonUk.Should().BeFalse();
    }
}
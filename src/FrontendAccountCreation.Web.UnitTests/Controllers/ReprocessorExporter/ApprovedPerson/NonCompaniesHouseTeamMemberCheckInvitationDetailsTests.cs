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

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberDetails_WhenIdIsNull_ShouldReturnEmptyViewModel()
    {
        // Arrange
        Guid? id = null;
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(id);
        var model = ((ViewResult)result).Model.As<NonCompaniesHouseTeamMemberViewModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().BeNull();
        model.FirstName.Should().BeNull();
        model.LastName.Should().BeNull();
        model.Telephone.Should().BeNull();
        model.Email.Should().BeNull();
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberDetails_WhenIdIsProvidedButNoTeamMemberExists_ShouldReturnEmptyViewModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        var teamMemberId = Guid.NewGuid();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(teamMemberId);
        var model = ((ViewResult)result).Model.As<NonCompaniesHouseTeamMemberViewModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().BeNull();
        model.FirstName.Should().BeNull();
        model.LastName.Should().BeNull();
        model.Telephone.Should().BeNull();
        model.Email.Should().BeNull();
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberDetails_WhenIdIsProvidedAndTeamMemberExists_ShouldPopulateViewModel()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = teamMemberId,
                    FirstName = "Alice",
                    LastName = "Johnson",
                    TelephoneNumber = "0123456789",
                    Email = "alice@example.com"
                }
            }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(teamMemberId);
        var model = ((ViewResult)result).Model.As<NonCompaniesHouseTeamMemberViewModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().Be(teamMemberId);
        model.FirstName.Should().Be("Alice");
        model.LastName.Should().Be("Johnson");
        model.Telephone.Should().Be("0123456789");
        model.Email.Should().Be("alice@example.com");
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberDetailsPost_WhenModelIsValid_ShouldSaveTeamMemberAndRedirect()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        var model = new NonCompaniesHouseTeamMemberViewModel
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Telephone = "01234567890",
            Email = "john.doe@example.com"
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);
        _sessionManagerMock.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));

        // Verify the session was updated
        session.ReExManualInputSession.TeamMembers.Should().ContainSingle(m => m.FirstName == "John" && m.LastName == "Doe");
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberDetailsPost_WhenModelIsInvalid_ShouldReturnToViewWithModelStateErrors()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        var model = new NonCompaniesHouseTeamMemberViewModel
        {
            Id = Guid.NewGuid(),
            FirstName = "",
            LastName = "Doe",
            Telephone = "01234567890",
            Email = "john.doe@example.com"
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _systemUnderTest.ModelState.AddModelError("FirstName", "First name is required");

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task NonCompaniesHouseTeamMemberDetails_WhenModelIsInvalid_ReturnsViewWithModel()
    {
        // Arrange
        var model = new NonCompaniesHouseTeamMemberViewModel
        {
            Id = Guid.NewGuid(),
            FirstName = null,
            LastName = "",
            Telephone = "",
            Email = ""
        };

        _systemUnderTest.ModelState.AddModelError("FirstName", "First name is required");
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession { TeamMembers = new List<ReExCompanyTeamMember>() }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Never); // Ensure SaveSessionAsync is not called
    }

}
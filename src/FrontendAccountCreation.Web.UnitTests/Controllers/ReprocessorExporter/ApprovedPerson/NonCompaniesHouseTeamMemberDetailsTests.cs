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
public class NonCompaniesHouseTeamMemberDetailsTests : ApprovedPersonTestBase
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
                PagePath.SoleTrader,
                PagePath.NonCompaniesHouseTeamMemberDetails
            ],
            ReExManualInputSession = new ReExManualInputSession(),
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task GET_WhenTeamMemberIsNotInSession_ThenViewIsReturnedWithoutTeamMemberDetails()
    {
        //Act
        var id = Guid.NewGuid();
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(id);

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<NonCompaniesHouseTeamMemberViewModel>();
        var viewModel = (NonCompaniesHouseTeamMemberViewModel?)viewResult.Model;
        viewModel!.FirstName.Should().BeNull();
        viewModel.LastName.Should().BeNull();
        viewModel.Email.Should().BeNull();
        viewModel.Telephone.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_WhenTeamMemberIsInSession_ThenViewIsReturnedWithTeamMemberDetails()
    {
        //Arrange
        _orgSessionMock!.ReExManualInputSession = new ReExManualInputSession
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
        };

        //Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(_orgSessionMock.ReExManualInputSession.TeamMembers[0].Id);

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<NonCompaniesHouseTeamMemberViewModel>();
        var viewModel = (NonCompaniesHouseTeamMemberViewModel?)viewResult.Model;
        viewModel!.FirstName.Should().Be("John");
        viewModel.LastName.Should().Be("Smith");
        viewModel.Email.Should().Be("teammember@email.com");
        viewModel.Telephone.Should().Be("01234567890");
    }

    [TestMethod]
    public async Task GET_ThenBackLinkIsCorrect()
    {
        //Act
        var id = Guid.NewGuid();
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(id);

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.SoleTrader);
    }

    [TestMethod]
    public async Task POST_GivenTeamMemberDetails_ThenRedirectToNonCompaniesHouseTeamMemberCheckInvitationDetails()
    {
        // Arrange
        var request = new NonCompaniesHouseTeamMemberViewModel
        {
            FirstName = "Teddy",
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));
    }

    [TestMethod]
    public async Task POST_GivenTeamMemberDetails_ThenUpdatesSession()
    {
        // Arrange
        var request = new NonCompaniesHouseTeamMemberViewModel
        {
            FirstName = "Teddy",
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession != null &&
                s.ReExManualInputSession.TeamMembers != null &&
                s.ReExManualInputSession.TeamMembers.Any(tm =>
                    tm.FirstName == "Teddy" &&
                    tm.LastName == "Drowns" &&
                    tm.Email == "teammember@example.com" &&
                    tm.TelephoneNumber == "01234567890"
                )
            )),
            Times.Once
        );
    }


    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenSessionNotUpdated()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonCompaniesHouseTeamMemberViewModel.FirstName), "Enter their first name");

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(new NonCompaniesHouseTeamMemberViewModel());

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenReturnView()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonCompaniesHouseTeamMemberViewModel.FirstName), "Enter their first name");
        var viewModel = new NonCompaniesHouseTeamMemberViewModel
        {
            FirstName = null,
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenReturnViewWithUsersBadInput()
    {
        // Arrange
        const string tooLongFirstName = "123456789 123456789 123456789 123456789 123456789 1";

        _systemUnderTest.ModelState.AddModelError(nameof(NonCompaniesHouseTeamMemberViewModel.FirstName), "First name must be 50 characters or less");
        var viewModel = new NonCompaniesHouseTeamMemberViewModel
        {
            FirstName = tooLongFirstName,
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<NonCompaniesHouseTeamMemberViewModel?>();
        var resultViewModel = (NonCompaniesHouseTeamMemberViewModel?)viewResult.Model;
        resultViewModel!.FirstName.Should().Be(tooLongFirstName);
    }

    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenViewHasCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(NonCompaniesHouseTeamMemberViewModel.FirstName), "Enter their first name");

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(new NonCompaniesHouseTeamMemberViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.SoleTrader);
    }

    [TestMethod]
    public async Task POST_NonCompaniesHouseTeamMemberDetails_WhenExistingMember_UpdatesMemberDetails()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingMember = new ReExCompanyTeamMember
        {
            Id = existingId,
            FirstName = "OldFirst",
            LastName = "OldLast",
            TelephoneNumber = "0000000000",
            Email = "old@example.com"
        };

        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember> { existingMember }
            }
        };

        var model = new NonCompaniesHouseTeamMemberViewModel
        {
            Id = existingId,
            FirstName = "NewFirst",
            LastName = "NewLast",
            Telephone = "1234567890",
            Email = "new@example.com"
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(model);

        // Assert
        var updatedMember = session.ReExManualInputSession.TeamMembers.First();
        updatedMember.FirstName.Should().Be("NewFirst");
        updatedMember.LastName.Should().Be("NewLast");
        updatedMember.TelephoneNumber.Should().Be("1234567890");
        updatedMember.Email.Should().Be("new@example.com");

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(nameof(_systemUnderTest.NonCompaniesHouseTeamMemberCheckInvitationDetails));
    }

    [TestMethod]
    public async Task POST_NonCompaniesHouseTeamMemberDetails_WhenIdMatches_UpdatesExistingMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();

        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = memberId,
                    FirstName = "Old",
                    LastName = "Name"
                }
            }
            }
        };

        var model = new NonCompaniesHouseTeamMemberViewModel
        {
            Id = memberId,
            FirstName = "Updated",
            LastName = "User"
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(model);

        // Assert
        var updated = session.ReExManualInputSession.TeamMembers.Single();
        updated.FirstName.Should().Be("Updated");
        updated.LastName.Should().Be("User");
    }

    [TestMethod]
    public async Task Post_NonCompaniesHousePartnershipTeamMember_OnlyAdds_OneTeamMember_ForSoleTrader_UKAddressFlow()
    {
        // Arrange
        _orgSessionMock.IsUkMainAddress = true;
        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            ProducerType = ProducerType.SoleTrader,
            TeamMembers =
            [
                new() {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "director@gmail.com"
                },
                new() {
                    FirstName = "Teddy",
                    LastName = "Smith",
                    Id = Guid.NewGuid(),
                    Email = "teddy.smith@gmail.com"
                }
            ]
        };

        var model = new NonCompaniesHouseTeamMemberViewModel
        {
            FirstName = "Teddy",
            LastName = "Smith",
            Telephone = "1234567890",
            Email = "teddy.smith@gmail.com"
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(model);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers.Count == 1 &&
                s.ReExManualInputSession.TeamMembers[0].FirstName == "Teddy"
            )),
            Times.Once);
    }

    [TestMethod]
    [DataRow(null, null, 2)]
    [DataRow(false, ProducerType.SoleTrader, 2)]
    [DataRow(true, null, 2)]
    [DataRow(true, ProducerType.SoleTrader, 1)]
    [DataRow(true, ProducerType.SoleTrader, 0)] // set TeamMembers to null in session using expectedCount = 0
    [DataRow(true, ProducerType.LimitedPartnership, 2)]
    [DataRow(true, ProducerType.SoleTrader, 1, true)]
    public async Task Post_NonCompaniesHousePartnershipTeamMember_Adds_OneTeamMember_ForSoleTrader_NonUKAddressFlow(bool? isUkMainAddress, ProducerType? producerType, int expectedCount, bool isCountZero = false)
    {
        // Arrange
        _orgSessionMock.IsUkMainAddress = isUkMainAddress;
        _orgSessionMock.ReExManualInputSession = new ReExManualInputSession
        {
            ProducerType = producerType,
            TeamMembers = isCountZero ? [] :
            [
                new()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "director@gmail.com"
                }
            ]
        };

        if (expectedCount == 0)
        {
            _orgSessionMock.ReExManualInputSession.TeamMembers = null;
            expectedCount = 1;
        }

        var model = new NonCompaniesHouseTeamMemberViewModel
        {
            FirstName = "Teddy",
            LastName = "Smith",
            Id = Guid.NewGuid(),
            Email = "teddy.smith@gmail.com"
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails(model);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be(nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExManualInputSession.TeamMembers.Count == expectedCount
            )),
            Times.Once);
    }

}
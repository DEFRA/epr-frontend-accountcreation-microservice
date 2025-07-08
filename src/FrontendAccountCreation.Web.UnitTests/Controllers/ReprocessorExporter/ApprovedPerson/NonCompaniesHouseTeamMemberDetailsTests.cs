using FluentAssertions;
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
        // Arrnage
        var approvedPersonId = Guid.NewGuid();

        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(approvedPersonId);

        //Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails();

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
    public async Task GET_NonCompaniesHouseTeamMemberDetails_WhenIdIsNull_ShoulCreateEmptyViewModel()
    {
        // Arrange
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(null);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails();
        var model = ((ViewResult)result).Model.As<NonCompaniesHouseTeamMemberViewModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().NotBeEmpty();
        model.FirstName.Should().BeNull();
        model.LastName.Should().BeNull();
        model.Telephone.Should().BeNull();
        model.Email.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_NonCompaniesHouseTeamMemberDetails_WhenIdIsProvidedButNoTeamMemberExists_ShouldReturnEmptyViewModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>()
            }
        };

        var approvedPersonId = Guid.NewGuid();
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(approvedPersonId);

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails();
        var model = ((ViewResult)result).Model.As<NonCompaniesHouseTeamMemberViewModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().Be(approvedPersonId);
        model.FirstName.Should().BeNull();
        model.LastName.Should().BeNull();
        model.Telephone.Should().BeNull();
        model.Email.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_WhenTeamMemberIsInSession_ThenViewIsReturnedWithTeamMemberDetails()
    {
        //Arrange
        var approvedPersonId = Guid.NewGuid();
        _orgSessionMock!.ReExManualInputSession = new ReExManualInputSession
        {
            TeamMembers = new List<ReExCompanyTeamMember>
                {
                    new ReExCompanyTeamMember
                    {
                        Id = approvedPersonId,
                        FirstName = "John",
                        LastName = "Smith",
                        Email = "teammember@email.com",
                        TelephoneNumber = "01234567890"
                    }
                }
        };

        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(approvedPersonId);

        //Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails();

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
    public async Task GET_NonCompaniesHouseTeamMemberDetails_WhenIdIsProvidedAndTeamMemberExists_ShouldPopulateViewModel()
    {
        // Arrange
        var approvedPersonId = Guid.NewGuid();
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                TeamMembers = new List<ReExCompanyTeamMember>
            {
                new ReExCompanyTeamMember
                {
                    Id = approvedPersonId,
                    FirstName = "Alice",
                    LastName = "Johnson",
                    TelephoneNumber = "0123456789",
                    Email = "alice@example.com"
                }
            }
            }
        };

        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(approvedPersonId);

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails();
        var model = ((ViewResult)result).Model.As<NonCompaniesHouseTeamMemberViewModel>();

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().Be(approvedPersonId);
        model.FirstName.Should().Be("Alice");
        model.LastName.Should().Be("Johnson");
        model.Telephone.Should().Be("0123456789");
        model.Email.Should().Be("alice@example.com");
    }

    [TestMethod]
    public async Task GET_ThenBackLinkIsCorrect()
    {
        //Act
        _tempDataDictionaryMock.Setup(dictionary => dictionary["FocusId"]).Returns(Guid.NewGuid());

        var result = await _systemUnderTest.NonCompaniesHouseTeamMemberDetails();

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
    public async Task POST_NonCompaniesHouseTeamMemberDetails_WhenModelIsValid_ShouldSaveTeamMemberAndRedirect()
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
    public async Task POST_NonCompaniesHouseTeamMemberDetails_WhenModelIsInvalid_ShouldReturnToViewWithModelStateErrors()
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
    public async Task POST_NonCompaniesHouseTeamMemberDetails_WhenModelIsInvalid_ReturnsViewWithModel()
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
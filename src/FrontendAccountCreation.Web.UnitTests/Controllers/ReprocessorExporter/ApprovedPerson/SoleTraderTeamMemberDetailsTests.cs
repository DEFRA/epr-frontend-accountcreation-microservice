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
public class SoleTraderTeamMemberDetailsTests : ApprovedPersonTestBase
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
                PagePath.SoleTraderTeamMemberDetails
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
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<SoleTraderTeamMemberViewModel>();
        var viewModel = (SoleTraderTeamMemberViewModel?)viewResult.Model;
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
            TeamMember = new ReExCompanyTeamMember
            {
                FirstName = "Teddy",
                LastName = "Drowns",
                Email = "teammember@example.com",
                TelephoneNumber = "01234567890"
            }
        };

        //Act
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<SoleTraderTeamMemberViewModel>();
        var viewModel = (SoleTraderTeamMemberViewModel?)viewResult.Model;
        viewModel!.FirstName.Should().Be("Teddy");
        viewModel.LastName.Should().Be("Drowns");
        viewModel.Email.Should().Be("teammember@example.com");
        viewModel.Telephone.Should().Be("01234567890");
    }

    [TestMethod]
    public async Task GET_ThenBackLinkIsCorrect()
    {
        //Act
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.SoleTrader);
    }

    [TestMethod]
    public async Task POST_GivenTeamMemberDetails_ThenRedirectToTypeOfOrganisation()
    {
        // Arrange
        var request = new SoleTraderTeamMemberViewModel
        {
            FirstName = "Teddy",
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ApprovedPersonController.TeamMembersCheckInvitationDetails));
    }

    [TestMethod]
    public async Task POST_GivenTeamMemberDetails_ThenUpdatesSession()
    {
        // Arrange
        var request = new SoleTraderTeamMemberViewModel
        {
            FirstName = "Teddy",
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        // Act
        await _systemUnderTest.SoleTraderTeamMemberDetails(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()),
            Times.Once);
    }

    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenSessionNotUpdated()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(SoleTraderTeamMemberViewModel.FirstName), "Enter their first name");

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails(new SoleTraderTeamMemberViewModel());

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenReturnView()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(SoleTraderTeamMemberViewModel.FirstName), "Enter their first name");
        var viewModel = new SoleTraderTeamMemberViewModel
        {
            FirstName = null,
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenReturnViewWithUsersBadInput()
    {
        // Arrange
        const string tooLongFirstName = "123456789 123456789 123456789 123456789 123456789 1";

        _systemUnderTest.ModelState.AddModelError(nameof(SoleTraderTeamMemberViewModel.FirstName), "First name must be 50 characters or less");
        var viewModel = new SoleTraderTeamMemberViewModel
        {
            FirstName = tooLongFirstName,
            LastName = "Drowns",
            Email = "teammember@example.com",
            Telephone = "01234567890"
        };

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails(viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<SoleTraderTeamMemberViewModel?>();
        var resultViewModel = (SoleTraderTeamMemberViewModel?)viewResult.Model;
        resultViewModel!.FirstName.Should().Be(tooLongFirstName);
    }

    [TestMethod]
    public async Task POST_GivenMissingTeamMemberDetails_ThenViewHasCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(SoleTraderTeamMemberViewModel.FirstName), "Enter their first name");

        // Act
        var result = await _systemUnderTest.SoleTraderTeamMemberDetails(new SoleTraderTeamMemberViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.SoleTrader);
    }
}
using FluentAssertions;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Unincorporated;

[TestClass]
public class ManageAccountPersonTests : UnincorporatedTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.BusinessAddress, // assumed previous step
                PagePath.UnincorporatedManageControl,  // source of redirect
                PagePath.UnincorporatedManageAccountPerson  // target of redirect
            },
            ReExUnincorporatedFlowSession = new ReExUnincorporatedFlowSession
            {
                ManageAccountPersonAnswer = ManageAccountPersonAnswer.IAgreeToBeAnApprovedPerson
            }
        };


        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task ManageAccountPerson_Get_ReturnsExpectedViewAndBackLink()
    {
        var result = await _systemUnderTest.ManageAccountPerson();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<ReExManageAccountPersonViewModel>();

        AssertBackLink(viewResult, PagePath.UnincorporatedManageControl);
    }



    [TestMethod]
    public async Task ManageAccountPerson_Post_InvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var viewModel = new ReExManageAccountPersonViewModel(); // No answer set = invalid

        _systemUnderTest.ModelState.AddModelError("ManageAccountPersonAnswer", "Required");

        // Act
        var result = await _systemUnderTest.ManageAccountPerson(viewModel);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var returnedModel = viewResult.Model.Should().BeOfType<ReExManageAccountPersonViewModel>().Subject;
        returnedModel.Should().Be(viewModel);
        AssertBackLink(viewResult, PagePath.UnincorporatedManageControl);
    }

    [TestMethod]
    public async Task ManageAccountPerson_Post_IAgreeToBeAnApprovedPerson_RedirectsToApprovedPerson()
    {
        // Arrange
        var viewModel = new ReExManageAccountPersonViewModel
        {
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IAgreeToBeAnApprovedPerson
        };

        // Act
        var result = await _systemUnderTest.ManageAccountPerson(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ApprovedPerson));
    }

    [TestMethod]
    public async Task ManageAccountPerson_Post_IWillInviteATeamMember_RedirectsToManageControl()
    {
        // Arrange
        var viewModel = new ReExManageAccountPersonViewModel
        {
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IWillInviteATeamMemberToBeApprovedPersonInstead
        };

        // Act
        var result = await _systemUnderTest.ManageAccountPerson(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ManageControl));
    }

    [TestMethod]
    public async Task ManageAccountPerson_Post_IWillInviteLater_RedirectsToManageControl()
    {
        // Arrange
        var viewModel = new ReExManageAccountPersonViewModel
        {
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IWillInviteAnApprovedPersonLater
        };

        // Act
        var result = await _systemUnderTest.ManageAccountPerson(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ManageControl));
    }

    [TestMethod]
    public async Task ManageAccountPersonUserFromTeam_Post_ValidAnswer_RedirectsToManageControl()
    {
        var viewModel = new ReExManageAccountPersonUserFromTeamViewModel
        {
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IWillInviteATeamMemberToBeApprovedPersonInstead
        };

        var result = await _systemUnderTest.ManageAccountPersonUserFromTeam(viewModel);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ManageControl));
    }

    [TestMethod]
    public async Task ManageAccountPersonUserFromTeam_Post_SetsSessionCorrectly()
    {
        var viewModel = new ReExManageAccountPersonUserFromTeamViewModel
        {
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IWillInviteAnApprovedPersonLater
        };

        await _systemUnderTest.ManageAccountPersonUserFromTeam(viewModel);

        _organisationSession.ReExUnincorporatedFlowSession.ManageAccountPersonAnswer
            .Should().Be(ManageAccountPersonAnswer.IWillInviteAnApprovedPersonLater);
    }

    [TestMethod]
    public async Task ManageAccountPersonUserFromTeam_Post_InvalidModel_ReturnsViewWithModel()
    {
        _organisationSession.Journey = new List<string>
        {
            PagePath.UnincorporatedManageControl,
            PagePath.UnincorporatedManageAccountPersonUserFromTeam
        };

        var viewModel = new ReExManageAccountPersonUserFromTeamViewModel();
        _systemUnderTest.ModelState.AddModelError("ManageAccountPersonAnswer", "Required");

        // Act
        var result = await _systemUnderTest.ManageAccountPersonUserFromTeam(viewModel);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var returnedModel = viewResult.Model.Should().BeOfType<ReExManageAccountPersonUserFromTeamViewModel>().Subject;
        returnedModel.Should().Be(viewModel);

        AssertBackLink(viewResult, PagePath.UnincorporatedManageControl);
    }
    [TestMethod]
    public async Task ManageAccountPersonUserFromTeam_Get_ReturnsExpectedViewAndBackLink()
    {
        _organisationSession.Journey = new List<string>
        {
            PagePath.UnincorporatedManageControl,
            PagePath.UnincorporatedManageAccountPersonUserFromTeam
        };

        // Act
        var result = await _systemUnderTest.ManageAccountPersonUserFromTeam();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<ReExManageAccountPersonUserFromTeamViewModel>();
        AssertBackLink(viewResult, PagePath.UnincorporatedManageControl);
    }

    [TestMethod]
    public async Task ManageAccountPersonUserFromTeam_Post_TeamMemberAnswer_RedirectsToManageControl()
    {
        // Arrange
        var viewModel = new ReExManageAccountPersonUserFromTeamViewModel
        {
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IWillInviteATeamMemberToBeApprovedPersonInstead
        };

        // Act
        var result = await _systemUnderTest.ManageAccountPersonUserFromTeam(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ManageControl));
        redirect.RouteValues.Should().BeNull("because SaveSessionAndRedirect does not currently set route values");
    }

    [TestMethod]
    public async Task ManageAccountPersonUserFromTeam_Post_InviteLaterAnswer_RedirectsToCheckYourDetails()
    {
        // Arrange
        _organisationSession = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.UnincorporatedManageControl,
                PagePath.UnincorporatedManageAccountPersonUserFromTeam,
                PagePath.UnincorporatedCheckYourDetails
            },
            ReExUnincorporatedFlowSession = new ReExUnincorporatedFlowSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);

        var viewModel = new ReExManageAccountPersonUserFromTeamViewModel
        {
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IWillInviteAnApprovedPersonLater
        };

        // Act
        var result = await _systemUnderTest.ManageAccountPersonUserFromTeam(viewModel);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.UnincorporatedCheckYourDetails));
    }
}

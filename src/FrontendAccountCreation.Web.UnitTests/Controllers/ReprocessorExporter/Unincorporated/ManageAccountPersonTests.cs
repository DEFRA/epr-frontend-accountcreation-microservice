using FluentAssertions;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
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
            ManageAccountPersonAnswer = ManageAccountPersonAnswer.IAgreeToBeAnApprovedPerson
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
    public async Task ManageAccountPerson_Post_IAgreeToBeAnApprovedPerson_RedirectsToManageControl()
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
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ManageControl));
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
}

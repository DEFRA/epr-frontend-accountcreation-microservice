using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Unincorporated;

[TestClass]
public class ManageControlOrganisationTests : UnincorporatedTestBase
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
                PagePath.UnincorporatedApprovedPerson,
                PagePath.UnincorporatedManageControlOrganisation
            },
            ReExUnincorporatedFlowSession = new ReExUnincorporatedFlowSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task ManageControlOrganisation_Get_ReturnsViewWithModelFromSession()
    {
        // Arrange
        var expectedAnswer = ManageControlAnswer.Yes;
        _organisationSession.ReExUnincorporatedFlowSession.ManageControlOrganisationAnswer = expectedAnswer;

        // Act
        var result = await _systemUnderTest.ManageControlOrganisation();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ReExManageControlOrganisationViewModel>().Subject;

        AssertBackLink(viewResult, PagePath.UnincorporatedApprovedPerson);
        model.Answer.Should().Be(expectedAnswer);
    }

    [TestMethod]
    public async Task ManageControlOrganisation_Post_WithInvalidInput_ReturnView()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError("Answer", "Test");

        var viewModel = new ReExManageControlOrganisationViewModel();

        // Act
        var result = await _systemUnderTest.ManageControlOrganisation(viewModel);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        AssertBackLink(viewResult, PagePath.UnincorporatedApprovedPerson);
        viewResult.Model.Should().Be(viewModel);
    }

    [TestMethod]
    public async Task ManageControlOrganisation_Post_ReturnRedirectToTeamMemberDetails()
    {
        // Arrange
        var answer = ManageControlAnswer.Yes;
        var viewModel = new ReExManageControlOrganisationViewModel { Answer = answer };

        // Act
        var result = await _systemUnderTest.ManageControlOrganisation(viewModel);

        // Assert
        // TODO: Update when controller redirects to correct page
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ManageAccountPerson));

        _organisationSession.ReExUnincorporatedFlowSession
            .ManageControlOrganisationAnswer
            .Should()
            .Be(answer);
    }

    [TestMethod]
    [DataRow(ManageControlAnswer.No)]
    [DataRow(ManageControlAnswer.NotSure)]
    public async Task ManageControlOrganisation_Post_ReturnRedirectToApprovedPersonCannotBeInvited(ManageControlAnswer answer)
    {
        // Arrange
        var viewModel = new ReExManageControlOrganisationViewModel { Answer = answer };

        // Act
        var result = await _systemUnderTest.ManageControlOrganisation(viewModel);

        // Assert
        // TODO: Update when controller redirects to correct page
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UnincorporatedController.ManageAccountPersonUserFromTeam));

        _organisationSession.ReExUnincorporatedFlowSession
            .ManageControlOrganisationAnswer
            .Should()
            .Be(answer);
    }
}
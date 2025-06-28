using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Unincorporated;

[TestClass]
public class ManageControlTests : UnincorporatedTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task ManageControl_Get_ReturnsViewWithModelFromSession()
    {
        // Arrange
        const ManageControlAnswer expectedAnswer = ManageControlAnswer.Yes;
        SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.ManageControl();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.Model, typeof(ReExManageControlViewModel));
        var model = (ReExManageControlViewModel)viewResult.Model;
        Assert.AreEqual(expectedAnswer, model.ManageControlInUKAnswer);
        AssertBackLink(viewResult, PagePath.UnincorporatedRoleInOrganisation);
    }

    [TestMethod]
    public async Task ManageControl_Post_InvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var viewModel = new ReExManageControlViewModel();
        SetupOrganisationSession();
        _systemUnderTest.ModelState.AddModelError("ManageControlInUKAnswer", "Required");

        // Act
        var result = await _systemUnderTest.ManageControl(viewModel);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.AreSame(viewModel, viewResult.Model);
        AssertBackLink(viewResult, PagePath.UnincorporatedRoleInOrganisation);
    }

    [TestMethod]
    public async Task ManageControl_Post_ValidModel_YesAnswer_RedirectsToManageAccountPerson()
    {
        // Arrange
        const ManageControlAnswer expectedAnswer = ManageControlAnswer.Yes;
        var viewModel = new ReExManageControlViewModel { ManageControlInUKAnswer = expectedAnswer };
        var session = SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.ManageControl(viewModel);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(UnincorporatedController.ManageAccountPerson), redirectResult.ActionName);
        _sessionManagerMock.Verify(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once());
    }

    [TestMethod]
    [DataRow(ManageControlAnswer.No)]
    [DataRow(ManageControlAnswer.NotSure)]
    public async Task ManageControl_Post_ValidModel_NotYesAnswer_RedirectsToManageAccountPerson(ManageControlAnswer expectedAnswer)
    {
        // Arrange
        var viewModel = new ReExManageControlViewModel { ManageControlInUKAnswer = expectedAnswer };
        var session = SetupOrganisationSession(expectedAnswer);

        // Act
        var result = await _systemUnderTest.ManageControl(viewModel);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(UnincorporatedController.ManageAccountPersonUserFromTeam), redirectResult.ActionName);
        Assert.AreEqual(expectedAnswer, session.ReExUnincorporatedFlowSession.ManageControlAnswer);
        _sessionManagerMock.Verify(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once());
    }

    private OrganisationSession SetupOrganisationSession(ManageControlAnswer expectedAnswer = ManageControlAnswer.Yes)
    {
        var session = new OrganisationSession
        {
            ReExUnincorporatedFlowSession = new ReExUnincorporatedFlowSession
            {
                ManageControlAnswer = expectedAnswer
            },
            Journey = [PagePath.UnincorporatedRoleInOrganisation, PagePath.UnincorporatedManageControl]
        };
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        return session;
    }
}
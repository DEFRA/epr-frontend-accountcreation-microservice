using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Unincorporated;

[TestClass]
public class ManageControlTests : UnincorporatedTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey = [PagePath.UnincorporatedRoleInOrganisation, PagePath.UnincorporatedManageControl]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
        _sessionManagerMock.Setup(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>())).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task ManageControl_Get_ReturnsViewWithModelFromSession()
    {
        // Arrange
        const ManageControlAnswer expectedAnswer = ManageControlAnswer.Yes;
        var session = new OrganisationSession
        {
            ManageControlAnswer = expectedAnswer, 
            Journey = [PagePath.UnincorporatedRoleInOrganisation, PagePath.UnincorporatedManageControl]
        };
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

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
        var viewModel = new ReExManageControlViewModel { ManageControlInUKAnswer = ManageControlAnswer.Yes };

        // Act
        var result = await _systemUnderTest.ManageControl(viewModel);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        // TODO: Update when ManageAccountPerson story completed
        //Assert.AreEqual(nameof(UnincorporatedController.ManageAccountPerson), redirectResult.ActionName);
        Assert.AreEqual(ManageControlAnswer.Yes, _organisationSession.ManageControlAnswer);
        _sessionManagerMock.Verify(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), _organisationSession), Times.Once());
    }

    [TestMethod]
    [DataRow(ManageControlAnswer.No)]
    [DataRow(ManageControlAnswer.NotSure)]
    public async Task ManageControl_Post_ValidModel_NotYesAnswer_RedirectsToManageAccountPerson(ManageControlAnswer expectedAnswer)
    {
        // Arrange
        var viewModel = new ReExManageControlViewModel { ManageControlInUKAnswer = expectedAnswer };

        // Act
        var result = await _systemUnderTest.ManageControl(viewModel);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        // TODO: Update when ManageAccountPerson story completed
        //Assert.AreEqual(nameof(UnincorporatedController.ManageAccountPerson), redirectResult.ActionName);
        Assert.AreEqual(expectedAnswer, _organisationSession.ManageControlAnswer);
        _sessionManagerMock.Verify(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), _organisationSession), Times.Once());
    }
}
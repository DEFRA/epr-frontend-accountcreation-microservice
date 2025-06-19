using FluentAssertions;
using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class ManageControlTests : OrganisationTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.AddressOverseas, PagePath.ManageControl
            ],
            ReExManualInputSession = new ReExManualInputSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task GET_BackLinkIsCorrect()
    {
        // Act
        var result = await _systemUnderTest.ManageControl();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.AddressOverseas);
    }

    [TestMethod]
    [DataRow(YesNoNotSure.Yes)]
    [DataRow(YesNoNotSure.No)]
    [DataRow(YesNoNotSure.NotSure)]
    [DataRow(null)]
    public async Task GET_CorrectViewModelIsReturnedInTheView(YesNoNotSure? sessionValue)
    {
        // Arrange
        _organisationSession.UserManagesOrControls = sessionValue;

        // Act
        var result = await _systemUnderTest.ManageControl();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ManageControlViewModel>();
        var viewModel = (ManageControlViewModel?)viewResult.Model;
        viewModel!.UserManagesOrControls.Should().Be(sessionValue);
    }

    [TestMethod]
    [DataRow(YesNoNotSure.Yes, nameof(OrganisationController.ManageAccountPerson), PagePath.ManageAccountPerson)]
    [DataRow(YesNoNotSure.No, nameof(OrganisationController.NotApprovedPerson), PagePath.NotApprovedPerson)]
    [DataRow(YesNoNotSure.NotSure, nameof(OrganisationController.NotApprovedPerson), PagePath.NotApprovedPerson)]
    public async Task POST_UserSelectsOption_RedirectsToCorrectPage(YesNoNotSure userAnswer, string expectedActionName, string expectedPagePath)
    {
        // Arrange
        var model = new ManageControlViewModel { UserManagesOrControls = userAnswer };

        // Act
        var result = await _systemUnderTest.ManageControl(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(expectedActionName);

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s => s.UserManagesOrControls == userAnswer &&
                                         s.Journey.Contains(PagePath.ManageControl) &&
                                         s.Journey.Contains(expectedPagePath))),
            Times.Once);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_SessionNotUpdated()
    {
        // Arrange
        var model = new ManageControlViewModel();
        _systemUnderTest.ModelState.AddModelError(nameof(ManageControlViewModel.UserManagesOrControls), "Select if you manage or control this organisation");

        // Act
        await _systemUnderTest.ManageControl(model);

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()), Times.Never);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_ViewIsReturnedWithCorrectModel()
    {
        // Arrange
        var model = new ManageControlViewModel();
        _systemUnderTest.ModelState.AddModelError(nameof(ManageControlViewModel.UserManagesOrControls), "Select if you manage or control this organisation");

        // Act
        var result = await _systemUnderTest.ManageControl(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ManageControlViewModel>();
        var viewModel = (ManageControlViewModel?)viewResult.Model;
        viewModel.Should().Be(model);
    }

    [TestMethod]
    public async Task POST_UserSelectsNothing_BackLinkIsCorrect()
    {
        // Arrange
        var model = new ManageControlViewModel();
        _systemUnderTest.ModelState.AddModelError(nameof(ManageControlViewModel.UserManagesOrControls), "Select if you manage or control this organisation");

        // Act
        var result = await _systemUnderTest.ManageControl(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.AddressOverseas);
    }

    [TestMethod]
    public async Task POST_UserSelectsOption_SessionIsUpdated()
    {
        // Arrange
        var model = new ManageControlViewModel { UserManagesOrControls = YesNoNotSure.Yes };

        // Act
        await _systemUnderTest.ManageControl(model);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s => s.UserManagesOrControls == YesNoNotSure.Yes)),
            Times.Once);
    }
}
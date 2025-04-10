namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using EPR.Common.Authorization.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

[TestClass]
public class ReExAccountTelephoneNumberTests : UserTestBase
{
    private ReExAccountCreationSession _reExAccountCreationSessionMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _reExAccountCreationSessionMock = new ReExAccountCreationSession
        {
            Journey = new List<string> { PagePath.FullName, PagePath.TelephoneNumber },
            Contact = new ReExContact()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_reExAccountCreationSessionMock);
    }

    [TestMethod]
    public async Task Get_ReExAccountTelephoneNumber_IsAllowed()
    {
        // Act
        var result = await _systemUnderTest.ReExAccountTelephoneNumber();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExAccountTelephoneNumberViewModel>();

        var reExAccountFullNameViewModel = (ReExAccountTelephoneNumberViewModel)viewResult.Model!;
        reExAccountFullNameViewModel.TelephoneNumber.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public async Task TelephoneNumber_PageIsSavedWithValidPhoneNumber_RedirectsToSuccess_AndUpdateSession()
    {
        // Arrange
        var request = new ReExAccountTelephoneNumberViewModel() { TelephoneNumber = "020 1234 5678" };

        // Act
        var result = await _systemUnderTest.ReExAccountTelephoneNumber(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(UserController.ReExAccountTelephoneNumber)); /*Success*/

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<ReExAccountCreationSession>()),
            Times.Once);
    }

    [TestMethod]
    public async Task TelephoneNumber_PageIsSavedWithInvalidPhoneNumber_ReturnsViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(ReExAccountTelephoneNumberViewModel.TelephoneNumber),
            "Field is invalid");

        var request = new ReExAccountTelephoneNumberViewModel() { TelephoneNumber = "" };

        // Act
        var result = await _systemUnderTest.ReExAccountTelephoneNumber(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<ReExAccountTelephoneNumberViewModel>();

        _sessionManagerMock.Verify(
            x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<ReExAccountCreationSession>>()), Times.Never);

        AssertBackLink(viewResult, PagePath.FullName);
    }

    [TestMethod]
    public async Task TelephoneNumber_TelephoneNumberPageIsExited_BackLinkIsFullName()
    {
        //Act
        var result = await _systemUnderTest.ReExAccountTelephoneNumber();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExAccountTelephoneNumberViewModel>();
        AssertBackLink(viewResult, PagePath.FullName);
    }
}
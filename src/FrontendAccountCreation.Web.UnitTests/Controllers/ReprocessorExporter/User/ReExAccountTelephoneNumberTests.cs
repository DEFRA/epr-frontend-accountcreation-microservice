namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.User;

using FluentAssertions;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
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

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(UserController.Success)); /*Success*/

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<ReExAccountCreationSession>()),
            Times.Once);
    }

    [TestMethod]
    public async Task TelephoneNumber_HappyPath_PostReprocessorExporterAccountAsyncOnFacadeCalled()
    {
        //Arrange
        var request = new ReExAccountTelephoneNumberViewModel() { TelephoneNumber = "020 1234 5678" };

        _reExAccountCreationSessionMock = new ReExAccountCreationSession
        {
            Journey = [PagePath.FullName, PagePath.TelephoneNumber],
            Contact = new ReExContact { FirstName = "Chris", LastName = "Stapleton" }
        };

        _reExAccountMapperMock.Setup(m => m.CreateReprocessorExporterAccountModel(
                It.IsAny<ReExAccountCreationSession>(), "email@example.com"))
            .Returns(new ReprocessorExporterAccountModel
            {
                Person = new PersonModel
                {
                    FirstName = "Chris",
                    LastName = "Stapleton",
                    ContactEmail = "email@example.com",
                    TelephoneNumber = "01234567890"
                }
            });

        //Act
        await _systemUnderTest.ReExAccountTelephoneNumber(request);

        //Assert
        _facadeServiceMock.Verify(f => f.PostReprocessorExporterAccountAsync(
            It.Is<ReprocessorExporterAccountModel>(m =>
                m.Person.FirstName == "Chris"
                && m.Person.LastName == "Stapleton"
                && m.Person.TelephoneNumber == "01234567890"
                && m.Person.ContactEmail == "email@example.com"
            ), ReExServiceKey), Times.Once);
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

    [TestMethod]
    public async Task ReExAccountTelephoneNumber_TelephoneNumberPageEntered_BackLinkIsSetAndViewModelPopulated()
    {
        // Arrange
        var testSession = new ReExAccountCreationSession
        {
            Contact = new ReExContact
            {
                TelephoneNumber = "0123456789",
                Email = "test@example.com"
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(testSession);

        // Act
        var result = await _systemUnderTest.ReExAccountTelephoneNumber();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExAccountTelephoneNumberViewModel>();

        var model = (ReExAccountTelephoneNumberViewModel)viewResult.Model;
        model.TelephoneNumber.Should().Be("0123456789");
        model.EmailAddress.Should().Be("test@example.com");

        var hasBackLinkKey = viewResult.ViewData.TryGetValue("BackLinkToDisplay", out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
    }
}
namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ReExUser;

using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Home;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

[TestClass]
public class ReExAccountFullNameTests : UserTestBase
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
    public async Task ReExAccountFullName_IfUserExistsAndAccountRedirectUrlIsNull_ThenRedirectsToUserAlreadyExistsPage()
    {
        //Arrange
        _facadeServiceMock.Setup(x => x.DoesAccountAlreadyExistAsync()).ReturnsAsync(true);

        // Act
        var result = await _systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(HomeController.UserAlreadyExists));
    }

    [TestMethod]
    public async Task ReExAccountFullName_IfUserExistsAndHasAccountRedirectUrl_ThenRedirectsToAccountRedirectUrl()
    {
        //Arrange
        var urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        var externalUrl = new ExternalUrlsOptions()
        {
            ExistingUserRedirectUrl = "dummy url"
        };


        _facadeServiceMock.Setup(x => x.DoesAccountAlreadyExistAsync()).ReturnsAsync(true);
        urlsOptionMock.Setup(x => x.Value)
            .Returns(externalUrl);
        var systemUnderTest = new UserController(_sessionManagerMock.Object, _facadeServiceMock.Object,
            _accountServiceMock.Object, urlsOptionMock.Object, _deploymentRoleOptionMock.Object, _loggerMock.Object);

        // Act
        var result = await systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().BeOfType<RedirectResult>();
        ((RedirectResult)result).Url.Should().Be("dummy url");
    }

    [TestMethod]
    public async Task Get_ReExAccountFullName_IsAllowed()
    {
        // Act
        var result = await _systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExAccountFullNameViewModel>();

        var reExAccountFullNameViewModel = (ReExAccountFullNameViewModel)viewResult.Model!;
        reExAccountFullNameViewModel.FirstName.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public async Task GivenFullNameProvided_WhenReExAccountFullNamePosted_ThenRedirectsToTelephoneNumberPage_AndUpdateSession()
    {
        // Arrange
        var request = new ReExAccountFullNameViewModel() { FirstName = "John", LastName = "Smith" };

        // Act
        var result = await _systemUnderTest.ReExAccountFullName(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(UserController.ReExAccountTelephoneNumber));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<ReExAccountCreationSession>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenNoFirstName_WhenReExAccountFullNamePosted_ThenReturnViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(ReExAccountFullNameViewModel.FirstName),
            "Field is required");

        var request = new ReExAccountFullNameViewModel() { LastName = "Smith" };

        // Act
        var result = await _systemUnderTest.ReExAccountFullName(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<ReExAccountFullNameViewModel>();

        _sessionManagerMock.Verify(
            x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<ReExAccountCreationSession>>()), Times.Never);
    }

    [TestMethod]
    public async Task GivenNoLastName_WhenReExAccountFullNamePosted_ThenReturnViewWithErrorAndCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(ReExAccountFullNameViewModel.LastName),
            "Field is required");

        var request = new ReExAccountFullNameViewModel() { FirstName = "John" };

        // Act
        var result = await _systemUnderTest.ReExAccountFullName(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<ReExAccountFullNameViewModel>();

        _sessionManagerMock.Verify(
            x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<ReExAccountCreationSession>>()), Times.Never);
    }
}
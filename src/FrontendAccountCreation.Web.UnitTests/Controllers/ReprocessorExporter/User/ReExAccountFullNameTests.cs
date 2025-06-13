namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.User;

using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
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
    public async Task Get_ReExAccountFullName_ReturnsViewModel_WithSessionData_FirstAndLastName()
    {
        // Arrange
        var sessionData = new ReExAccountCreationSession
        {
            Contact = new ReExContact
            {
                FirstName = "John",
                LastName = "Smith"
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                           .ReturnsAsync(sessionData);

        // Act
        var result = await _systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExAccountFullNameViewModel>();

        var viewModel = (ReExAccountFullNameViewModel)viewResult.Model!;
        viewModel.FirstName.Should().Be("John");
        viewModel.LastName.Should().Be("Smith");
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

    [TestMethod]
    [DataRow("John", "Smith")]
    [DataRow(null, null)]
    public async Task Get_ReExAccountFullName_ReturnsViewModel_WithSessionData_As(string firstName, string lastName)
    {
        // Arrange
        var session = new ReExAccountCreationSession
        {
            Contact = new ReExContact
            {
                FirstName = firstName,
                LastName = lastName
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExAccountFullNameViewModel>();

        var model = (ReExAccountFullNameViewModel)viewResult.Model!;
        model.FirstName.Should().Be(firstName);
        model.LastName.Should().Be(lastName);
    }

    [TestMethod]
    [DataRow("John", "Smith")]
    public async Task Post_ReExAccountFullName_WithValidModel_SavesToSessionAndRedirects_As(string firstName, string lastName)
    {
        // Arrange
        var session = new ReExAccountCreationSession
        {
            Contact = new ReExContact()
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var model = new ReExAccountFullNameViewModel
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var result = await _systemUnderTest.ReExAccountFullName(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        session.Contact.FirstName.Should().Be(firstName);
        session.Contact.LastName.Should().Be(lastName);
    }

    [TestMethod]
    public async Task Post_ReExAccountFullName_WithInvalidModel_ReturnsSameView()
    {
        // Arrange
        var model = new ReExAccountFullNameViewModel
        {
            FirstName = "",
            LastName = ""
        };

        _systemUnderTest.ModelState.AddModelError("FirstName", "Required");

        // Act
        var result = await _systemUnderTest.ReExAccountFullName(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task Get_ReExAccountFullName_SessionIsNull_DefaultSessionIsUsed()
    {
        // Arrange
        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((ReExAccountCreationSession)null!);

        // Act
        var result = await _systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExAccountFullNameViewModel>();

        var model = (ReExAccountFullNameViewModel)viewResult.Model!;
        model.FirstName.Should().BeNull();
        model.LastName.Should().BeNull();
    }

    [TestMethod]
    public async Task Get_ReExAccountFullName_ContactIsNull_ModelFieldsAreNull()
    {
        // Arrange
        var session = new ReExAccountCreationSession
        {
            Contact = null
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        var model = (ReExAccountFullNameViewModel)viewResult.Model!;
        model.FirstName.Should().BeNull();
        model.LastName.Should().BeNull();
    }

    [TestMethod]
    public async Task ReExAccountFullName_IfUserExists_ThenRedirectsToUserAlreadyExistsPage()
    {
        // Arrange
        var urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();

        _facadeServiceMock.Setup(x => x.DoesAccountAlreadyExistAsync()).ReturnsAsync(true);

        var systemUnderTest = new UserController(
            _sessionManagerMock.Object,
            _facadeServiceMock.Object,
            _reExAccountMapperMock.Object,
            urlsOptionMock.Object,
            _serviceKeysOptionsMock.Object,
            _loggerMock.Object
        );

        // Act
        var result = await systemUnderTest.ReExAccountFullName();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(UserController.ReExUserAlreadyExists));
    }
    
}
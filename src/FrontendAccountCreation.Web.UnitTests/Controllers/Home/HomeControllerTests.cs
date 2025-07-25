using FluentAssertions;
using FrontendAccountCreation.Web.Controllers.Home;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Home;

[TestClass]
public class HomeControllerTests
{
    private Mock<HttpContext>? _httpContextMock;
    private HomeController? _systemUnderTest;
    private Mock<ISession> _sessionMock = null!;
    private Mock<IResponseCookies>? _responseCookiesMock;

    [TestInitialize]
    public void Setup() {
        _responseCookiesMock = new Mock<IResponseCookies>();
        _httpContextMock = new Mock<HttpContext>();
        _sessionMock = new Mock<ISession>();

        _systemUnderTest = new HomeController();

        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;

        _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);

    }

    [TestMethod]
    public void UserAlreadyExists_ReturnsViewResult()
    {
        //Arrange
        var homeController = new HomeController();
        
        //Act
        var result = homeController.UserAlreadyExists();
        
        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
    }


    [TestMethod]
    [DataRow(null, "SignedOut")]    // default behaviour is to use existing signed out page
    [DataRow(false, "SignedOut")]
    [DataRow(true, "SignedOutReEx")]
    public void HomeController_SignedOut_RedirectsToCorrectSignedOutPage(bool? reEx, string expectedViewName)
    {
        //Arrange
        _httpContextMock!
           .Setup(x => x.Response.Cookies)
       .Returns(_responseCookiesMock!.Object);

        //Act
        var result = _systemUnderTest.SignedOut(reEx);

        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(expectedViewName);
    }

    [TestMethod]
    public void HomeController_SignedOutInvalidToken_When_Called_Return_View() 
    {
        //Act
        var result = _systemUnderTest.SignedOutInvalidToken();
        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();

    }

    [TestMethod]
    public void TimeoutSignedOut_ClearsSessionAndReturnsCorrectView()
    {
        // Arrange
        _sessionMock.Setup(s => s.Clear());

        // Act
        var result = _systemUnderTest.TimeoutSignedOut();

        // Assert
        _sessionMock.Verify(s => s.Clear(), Times.Once);
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("TimeoutSignedOut");
    }

    [TestMethod]
    public void SessionTimeoutModal_ReturnsTimeoutPartialView()
    {
        // Act
        var result = _systemUnderTest.SessionTimeoutModal();

        // Assert
        result.Should().BeOfType<PartialViewResult>();
        var partialResult = result as PartialViewResult;
        partialResult.ViewName.Should().Be("_TimeoutSessionWarning");
    }

}
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


    //[TestMethod]
    //public void HomeController_SignedOut_RedirectsToSigneOutPage()
    //{
    //    //Arrange
    //     _httpContextMock!
    //        .Setup(x => x.Response.Cookies)
    //    .Returns(_responseCookiesMock!.Object);
 
    //    //Act
    //    var result = _systemUnderTest.SignedOut();

    //    //Assert
    //    Assert.IsNotNull(result);
    //    result.Should().BeOfType<ViewResult>();
    //}

    [TestMethod]
    public void HomeController_SignedOutInvalidToken_When_Called_Return_View() 
    {
        //Act
        var result = _systemUnderTest.SignedOutInvalidToken();
        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();

    }

}
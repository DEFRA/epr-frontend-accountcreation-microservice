using System.Net;
using FluentAssertions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Error;

[TestClass]
public class AzureErrorControllerTests
{
    [TestMethod]
    public void Error_ReturnsCorrectResult()
    {
        //Arrange
        var _httpContextMock= new Mock<HttpContext>();
        var _httpResponse = new Mock<HttpResponse>();
        var errorController = new ErrorController();
        _httpContextMock.Setup(x => x.Response).Returns(_httpResponse.Object);
        errorController.ControllerContext.HttpContext = _httpContextMock.Object;
        //Act
        var result = errorController.Error((int)HttpStatusCode.NotFound);
        
        //Arrange
        result.Should().BeOfType(typeof(ViewResult));
    }

    [TestMethod]
    public void Error_ReturnsPageNotFoundView_WhenStatusCodeIs404()
    {
        // Arrange
        var controller = new AzureErrorController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = controller.Error((int?)HttpStatusCode.NotFound);

        // Assert
        result.Should().NotBeNull();
        result.ViewName.Should().Be(PagePath.PageNotFound);
        controller.Response.StatusCode.Should().Be(200);
    }

    [TestMethod]
    public void Error_ReturnsAErrorView_WhenStatusCodeIsNot404()
    {
        // Arrange
        var controller = new AzureErrorController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = controller.Error((int?)HttpStatusCode.InternalServerError);

        // Assert
        result.Should().NotBeNull();
        result.ViewName.Should().Be("AError");
        controller.Response.StatusCode.Should().Be(200);
    }

    [TestMethod]
    public void Error_ReturnsAErrorView_WhenStatusCodeIsNull()
    {
        // Arrange
        var controller = new AzureErrorController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = controller.Error(null);

        // Assert
        result.Should().NotBeNull();
        result.ViewName.Should().Be("AError");
        controller.Response.StatusCode.Should().Be(200);
    }

}
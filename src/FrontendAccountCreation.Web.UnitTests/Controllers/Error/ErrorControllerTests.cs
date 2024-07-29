using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using System.Net;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Error;

[TestClass]
public class ErrorControllerTests
{
    [TestMethod]
    public void Error_ReturnsPageNotFound_WhenStatusCodeIsNotFound()
    {
        // Arrange
        var httpContextMock = new Mock<HttpContext>();
        var httpResponseMock = new Mock<HttpResponse>();
        var errorController = new ErrorController();
        httpContextMock.Setup(x => x.Response).Returns(httpResponseMock.Object);
        errorController.ControllerContext.HttpContext = httpContextMock.Object;

        // Act
        var result = errorController.Error((int)HttpStatusCode.NotFound);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be(PagePath.PageNotFound);
        httpResponseMock.VerifySet(r => r.StatusCode = 200);
    }

    [TestMethod]
    public void Error_ReturnsErrorView_WhenStatusCodeIsNotNotFound()
    {
        // Arrange
        var httpContextMock = new Mock<HttpContext>();
        var httpResponseMock = new Mock<HttpResponse>();
        var errorController = new ErrorController();
        httpContextMock.Setup(x => x.Response).Returns(httpResponseMock.Object);
        errorController.ControllerContext.HttpContext = httpContextMock.Object;

        // Act
        var result = errorController.Error((int)HttpStatusCode.InternalServerError);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be("Error");
        httpResponseMock.VerifySet(r => r.StatusCode = 200);
    }
}

using System.Net;
using FluentAssertions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Error;

[TestClass]
public class ErrorControllerTests
{
    private ErrorController _systemUnderTest;
    private Mock<HttpResponse> _mockHttpResponse;

    [TestInitialize]
    public void Init()
    {
        var mockHttpContext = new Mock<HttpContext>();
        _mockHttpResponse = new Mock<HttpResponse>();

        _mockHttpResponse.SetupProperty(r => r.StatusCode, (int)HttpStatusCode.OK);
        mockHttpContext.Setup(c => c.Response).Returns(_mockHttpResponse.Object);

        var controllerContext = new ControllerContext()
        {
            HttpContext = mockHttpContext.Object
        };

        _systemUnderTest = new ErrorController
        {
            ControllerContext = controllerContext
        };
    }

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
    [DataRow(null)]
    [DataRow((int)HttpStatusCode.NotFound)]
    [DataRow((int)HttpStatusCode.InternalServerError)]
    [DataRow((int)HttpStatusCode.BadRequest)]
    public void Error_ReturnsErrorView_WhenCalledWithErrorCode(int? statusCode)
    {
        // Arrange
        var httpStatusCode = (HttpStatusCode?)statusCode;
        var expectedPageName = httpStatusCode switch
        {
            HttpStatusCode.NotFound => nameof(PagePath.PageNotFound),
            _ => "Error"
        };

        // Act
        var viewResult = _systemUnderTest.Error(statusCode);

        // Assert
        viewResult.ViewName.Should().Be(expectedPageName);

        if (statusCode == null)
        {
            _systemUnderTest.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }
        else
        {
            _systemUnderTest.Response.StatusCode.Should().Be(statusCode);

        }
    }

}
using System.Net;
using FluentAssertions;
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
    
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Net;
using FrontendAccountCreation.Core.Utilities;
using FrontendAccountCreation.Web.Controllers.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Error;

[TestClass]
public class ErrorControllerTests
{
    private AllowList<string>? _allowList;
    private Mock<HttpResponse> _httpResponse;
    private Mock<HttpContext> _httpContext;
    private Mock<IFeatureCollection> _featureCollection;
    private Mock<IExceptionHandlerPathFeature> _exceptionHandlerPathFeature;
    private Mock<IStatusCodeReExecuteFeature> _statusCodeReExecuteFeature;

    //todo: add to story about auth error page (and how to show it (create & cancel))
    //todo: add to story about generic page not found would need to be handled by 2 separate web apps with different config

    [TestInitialize]
    public void Setup()
    {
        _allowList = AllowList<string>.Create("User", "Organisation");
        _httpResponse = new Mock<HttpResponse>();
        _httpContext = new Mock<HttpContext>();
        _featureCollection = new Mock<IFeatureCollection>();
        _exceptionHandlerPathFeature = new Mock<IExceptionHandlerPathFeature>();
        _statusCodeReExecuteFeature = new Mock<IStatusCodeReExecuteFeature>();

        _httpContext.Setup(ctx => ctx.Features).Returns(_featureCollection.Object);

        _httpContext.Setup(x => x.Response).Returns(_httpResponse.Object);
    }

    [TestMethod]
    [DataRow("create-user/re-ex/not-a-page-ever", "PageNotFoundReEx")]
    [DataRow("Create-User/RE-EX/Not-A-Page-Ever", "PageNotFoundReEx")]
    [DataRow("create-user/Also-Not-A-Page-Ever", "PageNotFound")]
    public void Error_PageNotFound_WhenStatusCodeIsNotFound(string originalPath, string expectedViewName)
    {
        // Arrange
        _statusCodeReExecuteFeature.Setup(f => f.OriginalPath)
            .Returns(originalPath);
        _featureCollection.Setup(f => f.Get<IStatusCodeReExecuteFeature>()).Returns(_statusCodeReExecuteFeature.Object);

        var errorController = CreateErrorController();

        // Act
        var result = errorController.Error((int)HttpStatusCode.NotFound);

        // Assert
        result.ViewName.Should().Be(expectedViewName);
        //_httpResponse.VerifySet(r => r.StatusCode = 200);
    }

    [TestMethod]
    [DataRow("User", "error-reex")]
    [DataRow("Organisation", "error-reex")]
    [DataRow("Account", "error")]
    [DataRow("AccountCreation", "error")]
    public void Error_NotPageNotFoundGivenSourceController_CorrectViewIsReturned(string controllerName, string expectedViewName)
    {
        // Arrange
        _featureCollection.Setup(f => f.Get<IExceptionHandlerPathFeature>()).Returns(_exceptionHandlerPathFeature.Object);

        var routeValues = new RouteValueDictionary
        {
            { "Controller", controllerName }
        };

        _exceptionHandlerPathFeature.Setup(e => e.RouteValues).Returns(routeValues);

        var errorController = CreateErrorController();

        // Act
        var result = errorController.Error((int)HttpStatusCode.InternalServerError);

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(expectedViewName);
        //_httpResponse.VerifySet(r => r.StatusCode = 200);
    }

    private ErrorController CreateErrorController()
    {
        var errorController = new ErrorController(_allowList!);
        errorController.ControllerContext.HttpContext = _httpContext.Object;

        return errorController;
    }
}

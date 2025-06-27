using Microsoft.AspNetCore.Http;
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
    private Mock<HttpResponse>? _httpResponse;
    private Mock<HttpContext>? _httpContext;
    private Mock<IFeatureCollection>? _featureCollection;
    private Mock<IExceptionHandlerPathFeature>? _exceptionHandlerPathFeature;
    private Mock<IStatusCodeReExecuteFeature>? _statusCodeReExecuteFeature;

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
        _statusCodeReExecuteFeature!.Setup(f => f.OriginalPath)
            .Returns(originalPath);
        _featureCollection!.Setup(f => f.Get<IStatusCodeReExecuteFeature>()).Returns(_statusCodeReExecuteFeature.Object);

        var errorController = CreateErrorController();

        // Act
        var result = errorController.Error((int)HttpStatusCode.NotFound);

        // Assert
        result.ViewName.Should().Be(expectedViewName);
    }

    [TestMethod]
    [DataRow("create-user/re-ex/not-a-page-ever", "PageNotFoundReEx")]
    [DataRow("Create-User/RE-EX/Not-A-Page-Ever", "PageNotFoundReEx")]
    [DataRow("create-user/Also-Not-A-Page-Ever", "PageNotFound")]
    public void Error_PageNotFound_ReturnedStatusCodeIs404(string originalPath, string expectedViewName)
    {
        // Arrange
        _statusCodeReExecuteFeature!.Setup(f => f.OriginalPath)
            .Returns(originalPath);
        _featureCollection!.Setup(f => f.Get<IStatusCodeReExecuteFeature>()).Returns(_statusCodeReExecuteFeature.Object);

        var errorController = CreateErrorController();

        // Act
        errorController.Error((int)HttpStatusCode.NotFound);

        // Assert
        _httpResponse!.VerifySet(r => r.StatusCode = 404);
    }

    [TestMethod]
    [DataRow("User", "ErrorReEx")]
    [DataRow("Organisation", "ErrorReEx")]
    [DataRow("Account", "Error")]
    [DataRow("AccountCreation", "Error")]
    public void Error_NotPageNotFoundGivenSourceController_CorrectViewIsReturned(string controllerName, string expectedViewName)
    {
        // Arrange
        _featureCollection!.Setup(f => f.Get<IExceptionHandlerPathFeature>()).Returns(_exceptionHandlerPathFeature.Object);

        var routeValues = new RouteValueDictionary
        {
            { "Controller", controllerName }
        };

        _exceptionHandlerPathFeature.Setup(e => e.RouteValues).Returns(routeValues);

        var errorController = CreateErrorController();

        // Act
        var result = errorController.Error((int)HttpStatusCode.InternalServerError);

        // Assert
        result.ViewName.Should().Be(expectedViewName);
    }

    [TestMethod]
    [DataRow(null, 500, 500, false)]
    [DataRow(null, 500, 500, true)]
    [DataRow("User", null, 500)]
    [DataRow("Organisation", 500, 500)]
    [DataRow("Organisation", 500, 500, false, true)]
    [DataRow("Account", 403, 500)]
    [DataRow("User", 500, 500, true)]
    public void Error_NotPageNotFoundGivenSourceController_ReturnsSuppliedStatusCode(
        string? controllerName, int? passedStatusCode, int expectedStatusCode, bool mockFeatureReturnAsNull = false, bool hasEmptyRouteValues = false)
    {
        // Arrange
        var returnHandlerPathFeature = mockFeatureReturnAsNull ? (IExceptionHandlerPathFeature)null : _exceptionHandlerPathFeature!.Object;

        if ((controllerName == "User") && mockFeatureReturnAsNull)
        {
            _statusCodeReExecuteFeature!.Setup(f => f.OriginalPath)
                .Returns("somePath");
            _featureCollection!.Setup(f => f.Get<IStatusCodeReExecuteFeature>())
                .Returns(_statusCodeReExecuteFeature.Object);
        }

       _featureCollection!.Setup(f => f.Get<IExceptionHandlerPathFeature>()).Returns(returnHandlerPathFeature);

        var routeValues = hasEmptyRouteValues ? null : new RouteValueDictionary
        {
            { "Controller", controllerName }
        };

        _exceptionHandlerPathFeature.Setup(e => e.RouteValues).Returns(routeValues);

        var errorController = CreateErrorController();

        // Act
        errorController.Error(passedStatusCode);

        // Assert
        _httpResponse!.VerifySet(r => r.StatusCode = expectedStatusCode);
    }

    [TestMethod]
    public void ErrorReEx_ReturnsTheDefaultView()
    {
        // Arrange
        var errorController = CreateErrorController();

        // Act
        var result = errorController.ErrorReEx(null);

        // Assert
        result.ViewName.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null, 500)]
    [DataRow(500, 500)]
    [DataRow(400, 400)]
    public void ErrorReEx_GivenAStatusCode_ReturnsTheStatusCode(int? passedStatusCode, int expectedStatusCode)
    {
        // Arrange
        var errorController = CreateErrorController();

        // Act
        errorController.ErrorReEx(passedStatusCode);

        // Assert
        _httpResponse!.VerifySet(r => r.StatusCode = expectedStatusCode);
    }

    [TestMethod]
    public void PageNotFoundReEx_ReturnsTheDefaultView()
    {
        // Arrange
        var errorController = CreateErrorController();

        // Act
        var result = errorController.PageNotFoundReEx();

        // Assert
        result.ViewName.Should().BeNull();
    }

    [TestMethod]
    public void PageNotFoundReEx_Returns404()
    {
        // Arrange
        var errorController = CreateErrorController();

        // Act
        errorController.PageNotFoundReEx();

        // Assert
        _httpResponse!.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
    }

    private ErrorController CreateErrorController()
    {
        var errorController = new ErrorController(_allowList!);
        errorController.ControllerContext.HttpContext = _httpContext!.Object;

        return errorController;
    }
}

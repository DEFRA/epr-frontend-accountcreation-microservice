using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Middleware;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Middleware;

/// <summary>
/// Access means a page in the journey with the journey attribute.
/// No Access means the first page in the journey without the journey attribute.
/// Naming could be better, but we match JourneyAccessCheckerMiddlewareTests for consistency.
/// </summary>
[TestClass]
public class ReExJourneyAccessCheckerMiddlewareTests
{
    Mock<HttpContext> _httpContextMock = null!;
    Mock<HttpResponse> _httpResponseMock = null!;
    Mock<IFeatureCollection> _featureCollectionMock = null!;
    Mock<IEndpointFeature> _endpointFeatureMock = null!;
    Mock<ISessionManager<ReExAccountCreationSession>> _sessionManagerMock = null!;

    ReExJourneyAccessCheckerMiddleware _middleware = null!;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _featureCollectionMock = new Mock<IFeatureCollection>();
        _endpointFeatureMock = new Mock<IEndpointFeature>();
        _sessionManagerMock = new Mock<ISessionManager<ReExAccountCreationSession>>();

        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);
        _httpContextMock.Setup(x => x.Features).Returns(_featureCollectionMock.Object);
        _featureCollectionMock.Setup(x => x.Get<IEndpointFeature>()).Returns(_endpointFeatureMock.Object);

        _middleware = new ReExJourneyAccessCheckerMiddleware(_ => Task.CompletedTask);
    }

    [TestMethod]
    [DataRow(PagePath.TelephoneNumber, "/page-not-found-reex")]
    [DataRow(PagePath.TelephoneNumber, PagePath.PageNotFound, PagePath.PageNotFound)]
    public async Task GivenAccessRequiredPage_WhichIsNotPartOfTheVisitedURLs_WhenInvokeCalled_ThenRedirectedToExpectedPage
        (string pageUrl, string expectedPage, params string[] visitedUrls)
    {
        // Arrange
        var session = new ReExAccountCreationSession { Journey = visitedUrls.ToList() };
        var expectedUrl = expectedPage;

        SetupEndpointMock(new ReprocessorExporterJourneyAccessAttribute(pageUrl));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(expectedUrl), Times.Once);
    }

    [TestMethod]
    [DataRow(PagePath.TelephoneNumber, PagePath.FullName, PagePath.TelephoneNumber)]
    public async Task GivenAccessRequiredPage_WhichIsPartOfTheVisitedURLs_WhenInvokeCalled_ThenNoRedirectionHappened
        (string pageUrl, params string[] visitedUrls)
    {
        // Arrange
        var session = new ReExAccountCreationSession { Journey = visitedUrls.ToList() };

        SetupEndpointMock(new ReprocessorExporterJourneyAccessAttribute(pageUrl));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    [DataRow(PagePath.TelephoneNumber)]
    [DataRow(PagePath.Success)]
    public async Task GivenAccessRequiredPage_WithoutStoredSession_WhenInvokeCalled_ThenRedirectedToFirstPage(string pageUrl)
    {
        // Arrange
        const string firstPageUrl = PagePath.FullName;
        SetupEndpointMock(new ReprocessorExporterJourneyAccessAttribute(pageUrl));

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(firstPageUrl), Times.Once);
    }

    [TestMethod]
    [DataRow(PagePath.FullName)]
    public async Task GivenNoAccessRequiredPage_WhenInvokeCalled_ThenNoRedirectionHappened(string pageUrl)
    {
        // Arrange
        SetupEndpointMock();

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    private void SetupEndpointMock(params object[] attributes)
    {
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(attributes), null);

        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);
    }
}

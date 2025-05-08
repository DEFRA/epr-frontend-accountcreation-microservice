using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Middleware;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Middleware;

[TestClass]
public class OrganisationJourneyAccessCheckerMiddlewareTests
{
    Mock<HttpContext> _httpContextMock = null!;
    Mock<HttpResponse> _httpResponseMock = null!;
    Mock<IFeatureCollection> _featureCollectionMock = null!;
    Mock<IEndpointFeature> _endpointFeatureMock = null!;
    Mock<ISessionManager<OrganisationSession>> _sessionManagerMock = null!;

    OrganisationJourneyAccessCheckerMiddleware _middleware = null!;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _featureCollectionMock = new Mock<IFeatureCollection>();
        _endpointFeatureMock = new Mock<IEndpointFeature>();
        _sessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();

        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);

        _httpContextMock.Setup(x => x.Features).Returns(_featureCollectionMock.Object);
        _featureCollectionMock.Setup(x => x.Get<IEndpointFeature>()).Returns(_endpointFeatureMock.Object);

        _middleware = new OrganisationJourneyAccessCheckerMiddleware(_ => Task.CompletedTask);
    }

    [TestMethod]
    [DataRow(PagePath.NotAffected, "/page-not-found-reex")]
    [DataRow(PagePath.IsTradingNameDifferent, PagePath.RegisteredAsCharity, PagePath.RegisteredAsCharity)]
    public async Task GivenAccessRequiredPage_WhichIsNotPartOfTheVisitedURLs_WhenInvokeCalled_ThenRedirectedToExpectedPage
        (string pageUrl, string expectedPage, params string[] visitedUrls)
    {
        // Arrange
        var session = new OrganisationSession { Journey = visitedUrls.ToList() };
        var expectedUrl = expectedPage;

        SetupEndpointMock(new OrganisationJourneyAccessAttribute(pageUrl));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(expectedUrl), Times.Once);
    }

    [TestMethod]
    [DataRow(PagePath.NotAffected, PagePath.RegisteredAsCharity, PagePath.NotAffected)]
    public async Task GivenAccessRequiredPage_WhichIsPartOfTheVisitedURLs_WhenInvokeCalled_ThenNoRedirectionHappened
        (string pageUrl, params string[] visitedUrls)
    {
        // Arrange
        var session = new OrganisationSession { Journey = visitedUrls.ToList() };

        SetupEndpointMock(new OrganisationJourneyAccessAttribute(pageUrl));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    [DataRow(PagePath.NotAffected)]
    public async Task GivenAccessRequiredPage_WithoutStoredSession_WhenInvokeCalled_ThenRedirectedToFirstPage(string pageUrl)
    {
        // Arrange
        const string firstPageUrl = PagePath.RegisteredAsCharity;
        SetupEndpointMock(new OrganisationJourneyAccessAttribute(pageUrl));

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(firstPageUrl), Times.Once);
    }

    [TestMethod]
    [DataRow(PagePath.RegisteredAsCharity)]
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

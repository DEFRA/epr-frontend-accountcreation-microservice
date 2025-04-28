using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.FeatureManagement;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Middleware;

[TestClass]
public class FeatureMiddlewareTests
{
    private const string FeatureName = "feature-name";

    Mock<HttpContext> _httpContextMock = null!;
    Mock<HttpResponse> _httpResponseMock = null!;
    Mock<IFeatureCollection> _featureCollectionMock = null!;
    Mock<IFeatureManager>? _featureManagerMock;
    Mock<IEndpointFeature> _endpointFeatureMock = null!;

    FeatureMiddleware _middleware = null!;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _featureCollectionMock = new Mock<IFeatureCollection>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _endpointFeatureMock = new Mock<IEndpointFeature>();

        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);

        _httpContextMock.Setup(x => x.Features).Returns(_featureCollectionMock.Object);
        _featureCollectionMock.Setup(x => x.Get<IEndpointFeature>()).Returns(_endpointFeatureMock.Object);

        _middleware = new FeatureMiddleware(_ => Task.CompletedTask, _featureManagerMock.Object);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Invoke_PageDoesNotRequiresFeatureFlag_ThenNoRedirect(bool featureEnabled)
    {
        // Arrange
        //todo: in setup
        //todo: const string for featurename
        SetupEndpointMock();

        _featureManagerMock!.Setup(fm => fm.IsEnabledAsync(FeatureName))
            .ReturnsAsync(featureEnabled);

        // Act
        await _middleware.Invoke(_httpContextMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_PageRequiresFeatureFlagAndFlagIsEnabled_ThenNoRedirect()
    {
        // Arrange
        //todo: in setup
        //todo: const string for featurename
        SetupEndpointMock(new FeatureAttribute(FeatureName));

        _featureManagerMock!.Setup(fm => fm.IsEnabledAsync(FeatureName))
            .ReturnsAsync(true);

        // Act
        await _middleware.Invoke(_httpContextMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_PageRequiresFeatureFlagAndFlagIsDisabled_ThenRedirectToPageNotFound()
    {
        // Arrange
        _featureManagerMock!.Setup(fm => fm.IsEnabledAsync(FeatureName))
            .ReturnsAsync(false);

        SetupEndpointMock(new FeatureAttribute(FeatureName));

        // Act
        await _middleware.Invoke(_httpContextMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(PagePath.PageNotFound), Times.Once);
    }

    private void SetupEndpointMock(params object[] attributes)
    {
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(attributes), null);

        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);
    }
}

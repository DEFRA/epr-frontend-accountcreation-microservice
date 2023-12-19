using FrontendAccountCreation.Web.ViewComponents;
using FrontendAccountCreation.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.FeatureManagement;

namespace FrontendAccountCreation.Web.UnitTests.ViewComponents;

[TestClass]
public class LanguageSwitcherTests {
    private Mock<HttpContext> _httpContextMock = null!;
    private Mock<IFeatureCollection> _featureCollectionMock = null!;
    private Mock<IRequestCultureFeature> _requestCultureFeature = null!;
    
    private Mock<ISession> _sessionMock = null!;
    private Mock<IResponseCookies>? _responseCookiesMock;

    private Mock<HttpRequest> _httpRequest = null!;

    private Mock<RequestCulture> _requestCulure = null;

    private Mock<IFeatureManager> _featureManagerMock = null!;  


    [TestInitialize]
    public void Setup()
    {
        _httpRequest = new Mock<HttpRequest>();
        _requestCulure = new Mock<RequestCulture>("en-US");
          _responseCookiesMock = new Mock<IResponseCookies>();
        _httpContextMock = new Mock<HttpContext>();
        _sessionMock = new Mock<ISession>();
        _featureCollectionMock = new Mock<IFeatureCollection>();
        _requestCultureFeature = new Mock<IRequestCultureFeature>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _httpContextMock.Setup(x => x.Features).Returns(_featureCollectionMock.Object);
        _requestCultureFeature.Setup(x => x.RequestCulture).Returns(_requestCulure.Object);
        _featureCollectionMock.Setup(x => x.Get<IRequestCultureFeature>()).Returns(_requestCultureFeature.Object);
        _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);
     }

    [TestMethod]
    public async Task Invoke_SetsModelAsync()
    {

        // Arrange
        _httpContextMock!
           .Setup(x => x.Response.Cookies)
       .Returns(_responseCookiesMock!.Object);

        var viewContext = new ViewContext();
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequest.Object);
        viewContext.HttpContext = _httpContextMock.Object;

        string[] cultures = new string[2]
        {
            "en-US","en-US"
        };

        RequestLocalizationOptions requestLocalizationOptions = new RequestLocalizationOptions();

        var viewComponentContext = new ViewComponentContext();
        viewComponentContext.ViewContext = viewContext;
        requestLocalizationOptions.AddSupportedCultures(cultures);

        var options = Options.Create(requestLocalizationOptions);

        var component = new LanguageSwitcherViewComponent(options, _featureManagerMock.Object);
        component.ViewComponentContext = viewComponentContext;

        // Act
        var result = (ViewViewComponentResult)await component.InvokeAsync();

        var model = (LanguageSwitcherModel)(result.ViewData.Model!);

        // Assert
        Assert.IsNotNull(model);
        Assert.AreEqual("en-US", model.CurrentCulture.Name);
     }
}
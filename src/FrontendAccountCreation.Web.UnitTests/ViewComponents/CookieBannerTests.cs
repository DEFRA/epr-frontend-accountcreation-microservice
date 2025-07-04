using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.ViewComponents;
using FrontendAccountCreation.Web.ViewModels.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.ViewComponents;

[TestClass]
public class CookieBannerTests
{
    private Mock<HttpRequest> _httpRequest = null!;
    private Mock<HttpContext> _httpContextMock = null!;
    private Mock<ISession> _sessionMock = null!;
    private Mock<IResponseCookies>? _responseCookiesMock;
    private Mock<IRequestCookieCollection> _requestCookies;
    private Mock<ITempDataDictionary> _tempData;
    private Mock<RouteData> _routeData;

    [TestInitialize]
    public void Setup()
    {
        _httpRequest = new Mock<HttpRequest>();
        _httpContextMock = new Mock<HttpContext>();
        _responseCookiesMock = new Mock<IResponseCookies>();
        _requestCookies = new Mock<IRequestCookieCollection>();
        _tempData = new Mock<ITempDataDictionary>();
        _routeData = new Mock<RouteData>(); 

        _sessionMock = new Mock<ISession>();
        _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);
        _httpRequest.Setup(x => x.Cookies).Returns(_requestCookies.Object);
    }

    [TestMethod]
    public void Invoke_SetsModel()
    {
       // Arrange
        _httpContextMock!
           .Setup(x => x.Response.Cookies)
       .Returns(_responseCookiesMock!.Object);

        var viewContext = new ViewContext();
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequest.Object);
        viewContext.HttpContext = _httpContextMock.Object;

        viewContext.TempData = _tempData.Object;
        viewContext.RouteData = _routeData.Object;
        var viewComponentContext = new ViewComponentContext
        {
            ViewContext = viewContext,
     
        };

        var eprCookieOptions = new EprCookieOptions()
        {
           AuthenticationExpiryInMinutes = 60, AntiForgeryCookieName = null, AuthenticationCookieName = null, CookiePolicyCookieName = null, 
           CookiePolicyDurationInMonths = 60, SessionCookieName = null,  TempDataCookie = null, TsCookieName = null
        };
        var options = Options.Create(eprCookieOptions);
        var component = new CookieBannerViewComponent(options)
        {
            ViewComponentContext = viewComponentContext
        };

        var model = (CookieBannerModel)((ViewViewComponentResult)component.Invoke()).ViewData!.Model!;

        Assert.IsNotNull(model);
        Assert.IsTrue(model.ShowBanner);
    }
}
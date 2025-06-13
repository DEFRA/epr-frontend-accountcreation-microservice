using FrontendAccountCreation.IntegrationTests.Resources;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Cookies;
using FrontendAccountCreation.Web.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrontendAccountCreation.IntegrationTests.Controllers
{
    [TestClass]
    public class CookiesControllerTests
    {
        private Mock<ILogger<CookieService>> mockLogger;
        private IOptions<EprCookieOptions> eprCookieOptions;
        private IOptions<AnalyticsOptions> googleAnalyticsOptions;
        private CookieService cookieService;
        private CookiesController controller;
        private Mock<HttpContext> httpContextMock;
        private Mock<IResponseCookies> responseCookiesMock;
        private TestRequestCookieCollection requestCookieCollection;

        [TestInitialize]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<CookieService>>();
            httpContextMock = new Mock<HttpContext>();
            responseCookiesMock = new Mock<IResponseCookies>();


            eprCookieOptions = Options.Create(new EprCookieOptions()
            {
                CookiePolicyCookieName = ".epr_cookies_policy",
                CookiePolicyDurationInMonths = 24
            });

            googleAnalyticsOptions = Options.Create(new AnalyticsOptions
            {
                CookiePrefix = "ga_"
            });

            cookieService = new CookieService(mockLogger.Object, eprCookieOptions, googleAnalyticsOptions);
            controller = new CookiesController(cookieService);

            responseCookiesMock.Setup(r => r.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()));
            httpContextMock.SetupGet(c => c.Response.Cookies).Returns(responseCookiesMock.Object);
            httpContextMock.SetupGet(c => c.Request.Cookies).Returns(() => requestCookieCollection);

            requestCookieCollection = new TestRequestCookieCollection();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            controller.TempData = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
        }

        [TestMethod]
        public async Task CookiesController_UpdateAcceptance_ReturnsCorrectUrlRedirectResult()
        {
            string returnUrl = "TestReturnUrl";
            string cookieValue = CookieAcceptance.Accept;

            requestCookieCollection.Add("TestKey", "TestValue");

            // Act
            var result = controller.UpdateAcceptance(returnUrl, cookieValue);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LocalRedirectResult", result.GetType().Name);
            Assert.AreEqual(returnUrl, result.Url);
        }

        [TestMethod]
        public async Task CookiesController_UpdateAcceptance_WhenAcceptingCookies_DoesNotResetCookiesAndSetsEPRPolicyCookieAppropriately()
        {
            string returnUrl = "TestReturnUrl";
            string cookieValue = CookieAcceptance.Accept;

            requestCookieCollection.Add("ga_Test1", "TestValue");

            // Act
            var result = controller.UpdateAcceptance(returnUrl, cookieValue);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LocalRedirectResult", result.GetType().Name);
            Assert.AreEqual(returnUrl, result.Url);

            responseCookiesMock.Verify(r => r.Append("ga_Test1", It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Never);

            responseCookiesMock.Verify(r => r.Append(".epr_cookies_policy", "True", It.IsAny<CookieOptions>()), Times.Once);
        }

        [TestMethod]
        public async Task CookiesController_UpdateAcceptance_WhenRejectingCookies_ResetsCookiesExpirationAndSetsEPRPolicyCookieAppropriately()
        {
            string returnUrl = "TestReturnUrl";
            string cookieValue = CookieAcceptance.Reject;

            requestCookieCollection.Add("ga_Test1", "TestValue");

            // Act
            var result = controller.UpdateAcceptance(returnUrl, cookieValue);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LocalRedirectResult", result.GetType().Name);
            Assert.AreEqual(returnUrl, result.Url);

            var expectedExpiration = DateTime.UtcNow.AddYears(-1);
            responseCookiesMock.Verify(r => r.Append("ga_Test1", It.IsAny<string>(),
                It.Is<CookieOptions>(options => options.Expires.Value.Year == expectedExpiration.Year &&
                                                options.Expires.Value.Month == expectedExpiration.Month)), Times.Once);

            responseCookiesMock.Verify(r => r.Append(".epr_cookies_policy", "False", It.IsAny<CookieOptions>()), Times.Once);
        }

        [TestMethod]
        public async Task CookiesController_UpdateAcceptance_WhenRejectingCookies_DoesNotResetNonGoogleCookiesExpiration()
        {
            string returnUrl = "TestReturnUrl";
            string cookieValue = CookieAcceptance.Reject;

            requestCookieCollection.Add("ga_Test1", "TestValue");
            requestCookieCollection.Add("notgoogle_Test1", "TestValue");

            // Act
            var result = controller.UpdateAcceptance(returnUrl, cookieValue);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LocalRedirectResult", result.GetType().Name);
            Assert.AreEqual(returnUrl, result.Url);

            responseCookiesMock.Verify(r => r.Append("notgoogle_Test1", It.IsAny<string>(),
                It.IsAny<CookieOptions>()), Times.Never);

            var expectedExpiration = DateTime.UtcNow.AddYears(-1);
            responseCookiesMock.Verify(r => r.Append("ga_Test1", It.IsAny<string>(),
                It.Is<CookieOptions>(options => options.Expires.Value.Year == expectedExpiration.Year &&
                                                options.Expires.Value.Month == expectedExpiration.Month)), Times.Once);


            responseCookiesMock.Verify(r => r.Append(".epr_cookies_policy", "False", It.IsAny<CookieOptions>()), Times.Once);
        }

        [TestMethod]
        public async Task CookiesController_UpdateAcceptance_WhenExceptionThrown_LogsError()
        {
            string returnUrl = "TestReturnUrl";
            string cookieValue = CookieAcceptance.Reject;

            requestCookieCollection.Add("ga_Test1", "TestValue");

            responseCookiesMock.Setup(r => r.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()))
                .Throws(new Exception("Test Exception"));

            // Act
            try
            {
                var result = controller.UpdateAcceptance(returnUrl, cookieValue);
            }
            catch (Exception)
            {
                // Assert
                mockLogger.Verify(x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(), 
                    It.Is<Exception>(o => o.Message == "Test Exception"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
            }
        }

        // Add tests for AcknowledgeAcceptance method on the controller
        [TestMethod]
        public async Task CookiesController_AcknowledgeAcceptance_ReturnsCorrectUrlRedirectResult()
        {
            string returnUrl = "TestReturnUrl";

            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
            urlHelperMock.Setup(x => x.IsLocalUrl(returnUrl)).Returns(true);
            urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/");

            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.AcknowledgeAcceptance(returnUrl);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LocalRedirectResult", result.GetType().Name);
            Assert.AreEqual(returnUrl, result.Url);
        }

        [TestMethod]
        public async Task CookiesController_AcknowledgeAcceptance_WhenReturnUrlIsNotLocalUrl_ReturnsHomePath()
        {
            string returnUrl = "http://www.google.com";

            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
            urlHelperMock.Setup(x => x.IsLocalUrl(returnUrl)).Returns(false);
            urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.AcknowledgeAcceptance(returnUrl);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("LocalRedirectResult", result.GetType().Name);
            Assert.AreEqual("/", result.Url);
        }
    }
}
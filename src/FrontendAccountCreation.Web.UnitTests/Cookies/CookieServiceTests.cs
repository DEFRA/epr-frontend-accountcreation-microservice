﻿using FluentAssertions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Cookies;

[TestClass]
public class CookieServiceTests
{
    private const string CookieName = ".epr_cookies_policy";
    private const string GoogleAnalyticsDefaultCookieName = "_ga";

    private CookieService _systemUnderTest = null!;
    private Mock<IOptions<EprCookieOptions>> _cookieOptions = null!;
    private Mock<IOptions<AnalyticsOptions>> _googleAnalyticsOptions = null!;
    private Mock<ILogger<CookieService>> _loggerMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _cookieOptions = new Mock<IOptions<EprCookieOptions>>();
        _loggerMock = new Mock<ILogger<CookieService>>();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetCookieAcceptance_LogsError_WhenArgumentNullExceptionThrow()
    {
        // Arrange
        const string expectedLog = "Error setting cookie acceptance to 'True'";
        var requestCookieCollection = MockRequestCookieCollection("test", "test");
        HttpContext context = new DefaultHttpContext();
        MockService();

        // Act
        _systemUnderTest.SetCookieAcceptance(true, requestCookieCollection, context.Response.Cookies);

        // Assert
        _loggerMock.VerifyLog(logger => logger.LogError(expectedLog), Times.Once);
    }

    [TestMethod]
    public void SetCookieAcceptance_True_ReturnValidCookie()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection();
        var context = new DefaultHttpContext();
        MockService(CookieName);

        // Act
        _systemUnderTest.SetCookieAcceptance(true, requestCookieCollection, context.Response.Cookies);

        // Assert
        var cookieValue = GetCookieValueFromResponse(context.Response, CookieName);
        cookieValue.Should().Be("True");
    }

    [TestMethod]
    public void SetCookieAcceptance_False_ReturnValidCookie()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection();
        var context = new DefaultHttpContext();
        MockService(CookieName);

        // Act
        _systemUnderTest.SetCookieAcceptance(false, requestCookieCollection, context.Response.Cookies);

        // Assert
        var cookieValue = GetCookieValueFromResponse(context.Response, CookieName);
        cookieValue.Should().Be("False");
    }

    [TestMethod]
    public void SetCookieAcceptance_Return_False_Without_ExistingCookie()
    {
        // Arrange
        var context = new DefaultHttpContext();
        MockService(CookieName);

        // Act
        _systemUnderTest.SetCookieAcceptance(false, null, context.Response.Cookies);

        // Assert
        var cookieValue = GetCookieValueFromResponse(context.Response, CookieName);
        cookieValue.Should().Be("False");
    }

    [TestMethod]
    public void SetCookieAcceptance_False_ResetsGACookie()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection(GoogleAnalyticsDefaultCookieName, "1234");
        var context = new DefaultHttpContext();
        MockService(CookieName);

        // Act
        _systemUnderTest.SetCookieAcceptance(false, requestCookieCollection, context.Response.Cookies);

        // Assert
        var cookieValue = GetCookieValueFromResponse(context.Response, GoogleAnalyticsDefaultCookieName);
        cookieValue.Should().Be("1234");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void HasUserAcceptedCookies_LogsError_WhenArgumentNullExceptionThrow()
    {
        // Arrange
        const string expectedLog = "Error reading cookie acceptance";
        var requestCookieCollection = MockRequestCookieCollection("test", "test");
        MockService();

        // Act
        _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        _loggerMock.VerifyLog(logger => logger.LogError(expectedLog), Times.Once);
    }

    [TestMethod]
    public void HasUserAcceptedCookies_True_ReturnsValidValue()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection(CookieName, "True");
        MockService(CookieName);

        // Act
        var result = _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        result.Should().Be(true);
    }

    [TestMethod]
    public void HasUserAcceptedCookies_False_ReturnsValidValue()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection(CookieName, "False");
        MockService(CookieName);

        // Act
        var result = _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        result.Should().Be(false);
    }

    [TestMethod]
    public void HasUserAcceptedCookies_NoCookie_ReturnsValidValue()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection("test", "test");
        MockService(CookieName);

        // Act
        var result = _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        result.Should().Be(false);
    }

    private static IRequestCookieCollection MockRequestCookieCollection(string key = "", string value = "")
    {
        var requestFeature = new HttpRequestFeature();
        var featureCollection = new FeatureCollection();
        requestFeature.Headers = new HeaderDictionary();
        if (key != string.Empty && value != string.Empty)
        {
            requestFeature.Headers.Append(HeaderNames.Cookie, new StringValues(key + "=" + value));
        }

        featureCollection.Set<IHttpRequestFeature>(requestFeature);
        var cookiesFeature = new RequestCookiesFeature(featureCollection);
        return cookiesFeature.Cookies;
    }

    private static string? GetCookieValueFromResponse(HttpResponse response, string cookieName)
    {
        foreach (var headers in response.Headers)
        {
            if (headers.Key != "Set-Cookie")
            {
                continue;
            }

            string header = headers.Value;
            if (!header.StartsWith($"{cookieName}="))
            {
                continue;
            }

            var p1 = header.IndexOf('=');
            var p2 = header.IndexOf(';');
            return header.Substring(p1 + 1, p2 - p1 - 1);
        }

        return null;
    }

    private void MockService(string? cookieName = null)
    {
        var eprCookieOptions = new EprCookieOptions();
        if(cookieName != null)
        {
            eprCookieOptions.CookiePolicyCookieName = cookieName;   
        }
        var googleAnalyticsOptions = new AnalyticsOptions { CookiePrefix = GoogleAnalyticsDefaultCookieName };

        _cookieOptions = new Mock<IOptions<EprCookieOptions>>();
        _cookieOptions.Setup(ap => ap.Value).Returns(eprCookieOptions);

        _googleAnalyticsOptions = new Mock<IOptions<AnalyticsOptions>>();
        _googleAnalyticsOptions.Setup(ap => ap.Value).Returns(googleAnalyticsOptions);

        _systemUnderTest = new CookieService(
            _loggerMock.Object,
            _cookieOptions.Object,
            _googleAnalyticsOptions.Object);
    }
}
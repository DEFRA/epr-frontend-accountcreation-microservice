﻿using FluentAssertions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Culture;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace FrontendAccountCreation.Web.UnitTests.Controllers;

[TestClass]
public class CultureControllerTests
{
    private const string ReturnUrl = "returnUrl";

    private const string CultureEn = "en";

    private Mock<IResponseCookies>? _responseCookiesMock;
    private Mock<ISession> _sessionMock = null!;

    private Mock<HttpContext>? _httpContextMock;

    private CultureController? _systemUnderTest;

    [TestInitialize]
    public void Setup()
    {
        _responseCookiesMock = new Mock<IResponseCookies>();
        _sessionMock = new Mock<ISession>();
        _httpContextMock = new Mock<HttpContext>();
        _systemUnderTest = new CultureController();

        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;

        _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);
    }

    [TestMethod]
    public void CultureController_UpdateCulture_RedirectsToReturnUrlWithCulture()
    {
        // Arrange
        _httpContextMock!
            .Setup(x => x.Response.Cookies)
        .Returns(_responseCookiesMock!.Object);

        var cultureBytes = Encoding.UTF8.GetBytes(CultureEn);

        // Act
        var result = _systemUnderTest!.UpdateCulture(CultureEn, ReturnUrl);

        // Assert
        result.Url.Should().Be(ReturnUrl);

        _sessionMock.Verify(x => x.Set(Language.SessionLanguageKey, cultureBytes), Times.Once);
    }
}

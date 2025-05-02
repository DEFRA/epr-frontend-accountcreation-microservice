namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

using System.Security.Claims;
using Core.Services;
using Core.Sessions;
using FluentAssertions;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Web.Configs;

public abstract class UserTestBase
{
    protected const string BackLinkViewDataKey = "BackLinkToDisplay";
    protected const string ReExServiceKey = "ReprocessorExporter";

    protected Mock<HttpContext> _httpContextMock = null!;
    protected Mock<ISessionManager<ReExAccountCreationSession>> _sessionManagerMock = null!;
    protected Mock<IFacadeService> _facadeServiceMock = null!;
    protected Mock<IReExAccountMapper> _reExAccountMapperMock = null!;
    protected Mock<IOptions<ServiceKeysOptions>>? _serviceKeysOptionsMock;
    protected Mock<ILogger<UserController>> _loggerMock = null!;
    protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;

    protected UserController _systemUnderTest = null!;

    protected void SetupBase(string? deploymentRole = null)
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<ReExAccountCreationSession>>();
        _facadeServiceMock = new Mock<IFacadeService>();
        _reExAccountMapperMock = new Mock<IReExAccountMapper>();
        _serviceKeysOptionsMock = new Mock<IOptions<ServiceKeysOptions>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<ReExAccountCreationSession?>(new ReExAccountCreationSession()));

        _serviceKeysOptionsMock.Setup(x => x.Value)
            .Returns(new ServiceKeysOptions
            {
                ReprocessorExporter = ReExServiceKey
            });

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("Oid", Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "email@example.com")
        });

        _loggerMock = new Mock<ILogger<UserController>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _systemUnderTest = new UserController(_sessionManagerMock.Object, _facadeServiceMock.Object,
           _reExAccountMapperMock.Object, _serviceKeysOptionsMock.Object, _loggerMock.Object);

        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
        _systemUnderTest.TempData = _tempDataDictionaryMock.Object;
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}

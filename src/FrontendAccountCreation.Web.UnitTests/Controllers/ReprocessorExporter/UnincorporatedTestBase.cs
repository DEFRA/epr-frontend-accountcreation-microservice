namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using Web.Sessions;

/// <summary>
/// Used for Reprocessor and Exporter.
/// </summary>
public abstract class UnincorporatedTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    protected Mock<HttpContext> _httpContextMock = null!;
    protected Mock<ISessionManager<OrganisationSession>> _sessionManagerMock = null!;
    protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;
    protected UnincorporatedController _systemUnderTest = null!;

    protected void SetupBase()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<OrganisationSession?>(new OrganisationSession()));

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("Oid", Guid.NewGuid().ToString())
        });

        _systemUnderTest = new UnincorporatedController(_sessionManagerMock.Object);
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


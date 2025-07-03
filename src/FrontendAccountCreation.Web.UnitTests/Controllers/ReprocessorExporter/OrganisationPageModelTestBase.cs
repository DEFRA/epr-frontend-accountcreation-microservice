using Microsoft.AspNetCore.Mvc.RazorPages;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Pages.Organisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Moq;
using System.Security.Claims;
using FrontendAccountCreation.Web.Sessions;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

/// <summary>
/// Base test class for Reprocessor/Exporter OrganisationPageModel tests.
/// </summary>
public abstract class OrganisationPageModelTestBase<T> where T : OrganisationPageModel<T>
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    protected Mock<HttpContext> HttpContextMock = null!;
    protected PageContext PageContext = null!;
    protected Mock<ISessionManager<OrganisationSession>> SessionManagerMock = null!;
    protected Mock<IStringLocalizer<SharedResources>> SharedLocalizerMock = null!;
    protected Mock<IStringLocalizer<T>> LocalizerMock = null!;
    protected OrganisationSession OrganisationSession = new();

    protected void SetupBase()
    {
        HttpContextMock = new Mock<HttpContext>();
        SessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();
        SharedLocalizerMock = new Mock<IStringLocalizer<SharedResources>>();
        LocalizerMock = new Mock<IStringLocalizer<T>>();
        
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<OrganisationSession?>(OrganisationSession));

        SharedLocalizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, ""));

        LocalizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, ""));

        HttpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("Oid", Guid.NewGuid().ToString())
        });

        PageContext = CreatePageContext();
    }

    protected PageContext CreatePageContext()
    {
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

        return new PageContext
        {
            HttpContext = HttpContextMock.Object,
            ViewData = viewData
        };
    }

    //todo: use this
    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}


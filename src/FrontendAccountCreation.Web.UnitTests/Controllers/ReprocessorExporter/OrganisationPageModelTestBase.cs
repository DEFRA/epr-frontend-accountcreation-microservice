using Microsoft.AspNetCore.Mvc.RazorPages;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Moq;
using System.Security.Claims;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.Configs;
using Microsoft.Extensions.Options;

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
    protected Mock<IOptions<ExternalUrlsOptions>> UrlsOptionMock = null!;

    protected void SetupBase()
    {
        HttpContextMock = new Mock<HttpContext>();
        SessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();
        SharedLocalizerMock = new Mock<IStringLocalizer<SharedResources>>();
        LocalizerMock = new Mock<IStringLocalizer<T>>();
        UrlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        UrlsOptionMock.Setup(x => x.Value)
            .Returns(new ExternalUrlsOptions
            {
                ReportDataRedirectUrl = "/report-data",
                PrnRedirectUrl = "/epr-prn"
            });


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

    protected static void AssertBackLink(T page, string expectedBackLink)
    {
        page.ViewData[BackLinkViewDataKey].Should().Be(expectedBackLink);
    }
}


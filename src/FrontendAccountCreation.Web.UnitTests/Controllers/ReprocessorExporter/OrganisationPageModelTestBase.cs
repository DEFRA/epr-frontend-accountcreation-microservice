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
/// Used for Reprocessor and Exporter.
/// </summary>
public abstract class OrganisationPageModelTestBase<T> where T : OrganisationPageModel<T>
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    protected Mock<HttpContext> _httpContextMock = null!;
    protected PageContext PageContext;
    protected Mock<ISessionManager<OrganisationSession>> SessionManagerMock = null!;
    protected Mock<IStringLocalizer<SharedResources>> SharedLocalizerMock = null!;
    protected Mock<IStringLocalizer<T>> LocalizerMock = null!;
    protected OrganisationSession OrganisationSession = new();

    //protected Mock<IFacadeService> _facadeServiceMock = null!;
    //protected Mock<IReExAccountMapper> _reExAccountMapperMock = null!;
    //protected Mock<IOptions<DeploymentRoleOptions>> _deploymentRoleOptionMock = null!;
    //protected Mock<IFeatureManager> _featureManagerMock = null!;
    //protected Mock<ILogger<OrganisationController>> _loggerMock = null!;
    //protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;
    //protected OrganisationController _systemUnderTest = null!;

    //protected Mock<IMultipleOptions> _multipleOptionsMock = null!;

    protected void SetupBase() //string? deploymentRole = null)
    {
        _httpContextMock = new Mock<HttpContext>();
        SessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();
        SharedLocalizerMock = new Mock<IStringLocalizer<SharedResources>>();
        LocalizerMock = new Mock<IStringLocalizer<T>>();
        
        //_facadeServiceMock = new Mock<IFacadeService>();
        //_reExAccountMapperMock = new Mock<IReExAccountMapper>();
        //_multipleOptionsMock = new Mock<IMultipleOptions>();
        //_deploymentRoleOptionMock = new Mock<IOptions<DeploymentRoleOptions>>();
        //_featureManagerMock = new Mock<IFeatureManager>();
        //_tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        //_facadeServiceMock.Setup(f => f.GetOrganisationNameByInviteTokenAsync(It.IsAny<string>()))
        //    .ReturnsAsync(new ApprovedPersonOrganisationModel());
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<OrganisationSession?>(OrganisationSession));

        SharedLocalizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, ""));

        LocalizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, ""));

        //_multipleOptionsMock.Setup(x => x.UrlOptions)
        //    .Returns(new ExternalUrlsOptions
        //    {
        //        FindAndUpdateCompanyInformation = "dummy url",
        //        ReportDataRedirectUrl = "/re-ex",
        //        ReportDataLandingRedirectUrl = "/re-ex/landing",
        //        ReportDataNewApprovedUser = "/re-ex/approved-person-created?notification=created_new_approved_person"
        //    });

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("Oid", Guid.NewGuid().ToString())
        });

        PageContext = CreatePageContext();

        //_deploymentRoleOptionMock.Setup(x => x.Value)
        //    .Returns(new DeploymentRoleOptions
        //    {
        //        DeploymentRole = deploymentRole
        //    });

        //_multipleOptionsMock.Setup(x => x.ServiceKeysOptions)
        //   .Returns(new ServiceKeysOptions
        //   {
        //       ReprocessorExporter = "Re-Ex"
        //   });

        //_featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AddOrganisationSoleTraderJourney))
        //    .ReturnsAsync(true);

        //_loggerMock = new Mock<ILogger<OrganisationController>>();
        //_tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        //_systemUnderTest.TempData = _tempDataDictionaryMock.Object;
    }

    protected PageContext CreatePageContext()
    {
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

        return new PageContext
        {
            HttpContext = _httpContextMock.Object,
            ViewData = viewData
        };
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}


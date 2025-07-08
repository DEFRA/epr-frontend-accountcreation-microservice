namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

using Core.Services;
using Core.Services.FacadeModels;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using System.Security.Claims;
using Web.Configs;
using Web.Sessions;

/// <summary>
/// Used for Reprocessor and Exporter.
/// </summary>
public abstract class OrganisationTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    protected Mock<HttpContext> _httpContextMock = null!;
    protected Mock<ISessionManager<OrganisationSession>> _sessionManagerMock = null!;
    protected Mock<IFacadeService> _facadeServiceMock = null!;
    protected Mock<IReExAccountMapper> _reExAccountMapperMock = null!;
    protected Mock<IFeatureManager> _featureManagerMock = null!;
    protected Mock<ILogger<OrganisationController>> _loggerMock = null!;
    protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;
    protected OrganisationController _systemUnderTest = null!;

    protected Mock<IMultipleOptions> _multipleOptionsMock = null!;

    protected void SetupBase()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();
        _facadeServiceMock = new Mock<IFacadeService>();
        _reExAccountMapperMock = new Mock<IReExAccountMapper>();
        _multipleOptionsMock = new Mock<IMultipleOptions>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _facadeServiceMock.Setup(f => f.GetOrganisationNameByInviteTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApprovedPersonOrganisationModel());
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<OrganisationSession?>(new OrganisationSession()));

        _multipleOptionsMock.Setup(x => x.UrlOptions)
            .Returns(new ExternalUrlsOptions
            {
                FindAndUpdateCompanyInformation = "dummy url",
                ReportDataRedirectUrl = "/re-ex",
                ReportDataLandingRedirectUrl = "/re-ex/landing",
                ReportDataNewApprovedUser = "/re-ex/approved-person-created?notification=created_new_approved_person"
            });

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("Oid", Guid.NewGuid().ToString())
        });

        _multipleOptionsMock.Setup(x => x.ServiceKeysOptions)
           .Returns(new ServiceKeysOptions
           {
               ReprocessorExporter = "Re-Ex"
           });

        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.AddOrganisationSoleTraderJourney))
            .ReturnsAsync(true);

        _loggerMock = new Mock<ILogger<OrganisationController>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _systemUnderTest = new OrganisationController(
            _sessionManagerMock.Object,
            _facadeServiceMock.Object,
            _reExAccountMapperMock.Object,
            _multipleOptionsMock.Object,
           _loggerMock.Object);

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


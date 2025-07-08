namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

using Core.Services;
using Core.Services.FacadeModels;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using Web.Configs;
using Web.Sessions;

/// <summary>
/// Used for selection of approved team members - directors and company secretary.
/// </summary>
public abstract class ApprovedPersonTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    protected Mock<HttpContext> _httpContextMock = null!;
    protected Mock<ISessionManager<OrganisationSession>> _sessionManagerMock = null!;
    protected Mock<IFacadeService> _facadeServiceMock = null!;
    protected Mock<ICompanyService> _companyServiceMock = null!;
    protected Mock<IOptions<ExternalUrlsOptions>> _urlsOptionMock = null!;
    protected Mock<ILogger<ApprovedPersonController>> _loggerMock = null!;
    protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;
    protected Mock<IOptions<DeploymentRoleOptions>> _deploymentRoleOptionMock = null!;
    protected ApprovedPersonController _systemUnderTest = null!;

    protected void SetupBase(string? deploymentRole = null)
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();
        _facadeServiceMock = new Mock<IFacadeService>();
        _companyServiceMock = new Mock<ICompanyService>();
        _urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        _deploymentRoleOptionMock = new Mock<IOptions<DeploymentRoleOptions>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _facadeServiceMock.Setup(f => f.GetOrganisationNameByInviteTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApprovedPersonOrganisationModel());
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<OrganisationSession?>(new OrganisationSession()));

        _urlsOptionMock.Setup(x => x.Value)
            .Returns(new ExternalUrlsOptions
            {
                FindAndUpdateCompanyInformation = "dummy url",
                ReportDataRedirectUrl = "/re-ex",
                ReportDataLandingRedirectUrl = "/re-ex/landing",
                ReportDataNewApprovedUser = "/re-ex/approved-person-created?notification=created_new_approved_person",
                MakeChangesToYourLimitedCompany = "https://gov.uk/update-company-info" // ✅ Add this
            });

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("Oid", Guid.NewGuid().ToString())
        });

        _deploymentRoleOptionMock.Setup(x => x.Value)
            .Returns(new DeploymentRoleOptions
            {
                DeploymentRole = deploymentRole
            });

        _loggerMock = new Mock<ILogger<ApprovedPersonController>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _systemUnderTest = new ApprovedPersonController(_sessionManagerMock.Object, _urlsOptionMock.Object);

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
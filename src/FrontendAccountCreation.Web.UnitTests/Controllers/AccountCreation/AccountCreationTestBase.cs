namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

using System.Security.Claims;
using Core.Services;
using Core.Services.Dto.User;
using Core.Services.FacadeModels;
using Core.Sessions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Web.Configs;
using Web.Controllers.AccountCreation;
using Web.Sessions;

public abstract class AccountCreationTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    protected const string PostcodeLookupFailedKey = "PostcodeLookupFailed";
    protected Mock<HttpContext> _httpContextMock = null!;
    protected Mock<ISessionManager<AccountCreationSession>> _sessionManagerMock = null!;
    protected Mock<IFacadeService> _facadeServiceMock = null!;
    protected Mock<ICompanyService> _companyServiceMock = null!;
    protected Mock<IAccountMapper> _accountServiceMock = null!;
    protected Mock<IOptions<ExternalUrlsOptions>> _urlsOptionMock = null!;
    protected Mock<ILogger<AccountCreationController>> _loggerMock = null!;
    protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;
    protected Mock<IOptions<DeploymentRoleOptions>> _deploymentRoleOptionMock = null!;
    protected AccountCreationController _systemUnderTest = null!;

    protected void SetupBase(string? deploymentRole = null)
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<AccountCreationSession>>();
        _facadeServiceMock = new Mock<IFacadeService>();
        _companyServiceMock = new Mock<ICompanyService>();
        _accountServiceMock = new Mock<IAccountMapper>();
        _urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        _deploymentRoleOptionMock = new Mock<IOptions<DeploymentRoleOptions>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _facadeServiceMock.Setup(f => f.GetOrganisationNameByInviteTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApprovedPersonOrganisationModel());
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<AccountCreationSession?>(new AccountCreationSession()));

        _urlsOptionMock.Setup(x => x.Value)
            .Returns(new ExternalUrlsOptions
            {
                FindAndUpdateCompanyInformation = "dummy url",
                ReportDataRedirectUrl = "/report-data",
                ReportDataLandingRedirectUrl = "/report-data/landing",
                ReportDataNewApprovedUser = "/report-data/approved-person-created?notification=created_new_approved_person"
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

        _loggerMock = new Mock<ILogger<AccountCreationController>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _systemUnderTest = new AccountCreationController(_sessionManagerMock.Object, _facadeServiceMock.Object, _companyServiceMock.Object,
           _accountServiceMock.Object, _urlsOptionMock.Object, _deploymentRoleOptionMock.Object, _loggerMock.Object);

        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
        _systemUnderTest.TempData = _tempDataDictionaryMock.Object;
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
    
    protected static UserAccount CreateUserAccountModel(string enrolmentStatus) => new()
    {
        User = new User
        {
            Id = Guid.NewGuid(),
            EnrolmentStatus = enrolmentStatus
        }
    };
}

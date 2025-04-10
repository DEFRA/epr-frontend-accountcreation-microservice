using EPR.Common.Authorization.Sessions;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;

using System.Security.Claims;
using Core.Services;
using Core.Services.Dto.User;
using Core.Services.FacadeModels;
using Core.Sessions;
using FluentAssertions;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Web.Configs;

public abstract class UserTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    protected Mock<HttpContext> _httpContextMock = null!;
    protected Mock<ISessionManager<ReExAccountCreationSession>> _sessionManagerMock = null!;
    protected Mock<IFacadeService> _facadeServiceMock = null!;
    protected Mock<IReExAccountMapper> _reExAccountMapperMock = null!;
    protected Mock<IOptions<ExternalUrlsOptions>> _urlsOptionMock = null!;
    protected Mock<ILogger<UserController>> _loggerMock = null!;
    protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;

    protected UserController _systemUnderTest = null!;

    protected void SetupBase(string? deploymentRole = null)
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<ReExAccountCreationSession>>();
        _facadeServiceMock = new Mock<IFacadeService>();
        _reExAccountMapperMock = new Mock<IReExAccountMapper>();
        _urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        //_facadeServiceMock.Setup(f => f.GetOrganisationNameByInviteTokenAsync(It.IsAny<string>()))
        //    .ReturnsAsync(new ApprovedPersonOrganisationModel());
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<ReExAccountCreationSession?>(new ReExAccountCreationSession()));

        _urlsOptionMock.Setup(x => x.Value)
            .Returns(new ExternalUrlsOptions
            {
                FindAndUpdateCompanyInformation = "dummy url",
                ReportDataRedirectUrl = "/re-ex",
                ReportDataLandingRedirectUrl = "/re-ex/landing",
                ReportDataNewApprovedUser = "/re-ex/approved-person-created?notification=created_new_approved_person"
            });

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("Oid", Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "email@example.com")
        });

        _loggerMock = new Mock<ILogger<UserController>>();
        _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

        _systemUnderTest = new UserController(_sessionManagerMock.Object, _facadeServiceMock.Object,
           _reExAccountMapperMock.Object, _urlsOptionMock.Object, _loggerMock.Object);

        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
        _systemUnderTest.TempData = _tempDataDictionaryMock.Object;
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }

    //protected static UserAccount CreateUserAccountModel(string enrolmentStatus) => new()
    //{
    //    User = new Core.Services.Dto.User.User
    //    {
    //        Id = Guid.NewGuid(),
    //        EnrolmentStatus = enrolmentStatus
    //    }
    //};
}


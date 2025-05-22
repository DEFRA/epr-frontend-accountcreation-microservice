using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

public abstract class LimitedPartnershipTestBase
{
    protected OrganisationSession _orgSessionMock = null!;
    protected Mock<ISessionManager<OrganisationSession>> _sessionManagerMock = null!;
    protected LimitedPartnershipController _systemUnderTest = null!;

    protected void SetupBase()
    {
        _sessionManagerMock = new Mock<ISessionManager<OrganisationSession>>();
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim> { new("Oid", Guid.NewGuid().ToString()) });

        _systemUnderTest = new LimitedPartnershipController(_sessionManagerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };
    }
}
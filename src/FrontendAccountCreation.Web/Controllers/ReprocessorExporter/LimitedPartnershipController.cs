using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter
{
    [Route("re-ex/organisation")]
    public partial class LimitedPartnershipController : Controller
    {
        private readonly ISessionManager<OrganisationSession> _sessionManager;

        public LimitedPartnershipController(ISessionManager<OrganisationSession> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [HttpGet]
        [Route(PagePath.LimitedPartnershipNamesOfPartners)]
        public async Task<IActionResult> NamesOfPartners()
        {
            return View();
        }
    }
}
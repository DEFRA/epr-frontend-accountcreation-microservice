using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
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
            LimitedPartnershipPartnersViewModel model = new();
            var partnerList = new List<LimitedPartnershipPersonOrCompanyViewModel>();

            LimitedPartnershipPersonOrCompanyViewModel person = new LimitedPartnershipPersonOrCompanyViewModel();
            partnerList.Add(person);

            LimitedPartnershipPersonOrCompanyViewModel person2 = new LimitedPartnershipPersonOrCompanyViewModel();
            partnerList.Add(person2);
            model.Partners = partnerList;

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.LimitedPartnershipNamesOfPartners)]
        public async Task<IActionResult> NamesOfPartners(LimitedPartnershipPartnersViewModel model, string command)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return RedirectToAction("AddApprovedPerson", "ApprovedPerson");
        }
    }
}
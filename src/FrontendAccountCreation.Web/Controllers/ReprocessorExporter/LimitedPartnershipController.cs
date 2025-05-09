using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
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

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            ReExLimitedPartnership? ltdPartnershipSession = session.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership;
            model.ExpectsIndividualPartners = ltdPartnershipSession?.HasIndividualPartners ?? true;
            model.ExpectsCompanyPartners = ltdPartnershipSession?.HasCompanyPartners ?? true;

            List<ReExLimitedPartnershipPersonOrCompany>? partnersSession = ltdPartnershipSession?.Partners;
            List<LimitedPartnershipPersonOrCompanyViewModel> partnerList = [];
            if (partnersSession != null)
            {
                partnerList = partnersSession.Select(item => (LimitedPartnershipPersonOrCompanyViewModel)item)
                    .Where(x => ((x.IsPerson && model.ExpectsIndividualPartners) ||
                        (x.IsCompany && model.ExpectsCompanyPartners))).ToList();
            }

            if (partnerList.Count.Equals(0))
            {
                LimitedPartnershipPersonOrCompanyViewModel partner = new()
                {
                    Id = Guid.NewGuid()
                };
                partnerList.Add(partner);
                LimitedPartnershipPersonOrCompanyViewModel partner2 = new()
                {
                    Id = Guid.NewGuid()
                };
                partnerList.Add(partner2);
            }

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
using FrontendAccountCreation.Core.Extensions;
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
            // TODO: set back link when page becomes available

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
                    .Where(x => (
                            (!x.IsPersonOrCompanyButNotBoth) ||
                            (x.IsPerson && model.ExpectsIndividualPartners) ||
                            (x.IsCompany && model.ExpectsCompanyPartners)
                                )).ToList();
            }

            if (partnerList.Count.Equals(0))
            {
                LimitedPartnershipPersonOrCompanyViewModel newPartner = new()
                {
                    Id = Guid.NewGuid()
                };
                partnerList.Add(newPartner);
            }

            model.Partners = partnerList;
            return View(model);
        }

        /// <summary>
        /// Save partner details to session
        /// </summary>
        /// <param name="model">View model</param>
        /// <param name="command">'save' to update partners and continue, 'add' to add new partner, any Guid removes the corresponding item from the model.</param>
        /// <returns></returns>
        [HttpPost]
        [Route(PagePath.LimitedPartnershipNamesOfPartners)]
        public async Task<IActionResult> NamesOfPartners(LimitedPartnershipPartnersViewModel model, string command)
        {
            // when command is a Guid its an instruction to remove that item from the model, so do so before validating
            if (Guid.TryParse(command, out Guid removedId))
            {
                model.Partners.RemoveAll(x => x.Id == removedId);
                ModelState.Clear();
                if (!TryValidateModel(model, nameof(LimitedPartnershipPartnersViewModel)))
                {
                    // TODO: set back link when page becomes available
                    return View(model);
                }
            }
            else if (!ModelState.IsValid)
            {
                // TODO: set back link when page becomes available
                return View(model);
            }

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            ReExCompaniesHouseSession companiesHouseSession = session.ReExCompaniesHouseSession ?? new();
            ReExPartnership partnershipSession = companiesHouseSession.Partnership ?? new();

            // obtain partners from the view model
            List<ReExLimitedPartnershipPersonOrCompany> partners = await GetPartners(model);
            if (command == "add")
            {
                ReExLimitedPartnershipPersonOrCompany newPartner = new()
                {
                    Id = Guid.NewGuid()
                };

                partners.Add(newPartner);
            }

            // refresh limited partnership session from the view model
            ReExLimitedPartnership ltdPartnershipSession = new()
            {
                Partners = partners,
                HasCompanyPartners = model.ExpectsCompanyPartners,
                HasIndividualPartners = model.ExpectsIndividualPartners
            };

            partnershipSession.LimitedPartnership = ltdPartnershipSession;
            companiesHouseSession.Partnership = partnershipSession;
            session.ReExCompaniesHouseSession = companiesHouseSession;

            if (command == "save")
            {
                // TODO: swap this with SaveSessionAndRedirect() to correct page path
                await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
                return RedirectToAction("AddApprovedPerson", "ApprovedPerson");
            }
            else
            {
                return await SaveSessionAndRedirect(
                    session,
                    nameof(NamesOfPartners),
                    PagePath.LimitedPartnershipNamesOfPartners,
                    PagePath.LimitedPartnershipNamesOfPartners);
            }
        }

        private static async Task<List<ReExLimitedPartnershipPersonOrCompany>> GetPartners(LimitedPartnershipPartnersViewModel model)
        {
            List<ReExLimitedPartnershipPersonOrCompany> partnersSession = new();
            foreach (var partner in model.Partners)
            {
                ReExLimitedPartnershipPersonOrCompany sessionPartner = new()
                {
                    Id = partner.Id,
                    IsPerson = partner.IsPerson,
                    Name = partner.IsPerson ? partner.PersonName : partner.CompanyName
                };
                partnersSession.Add(sessionPartner);
            }

            return partnersSession;
        }

        private async Task<RedirectToActionResult> SaveSessionAndRedirect(OrganisationSession session,
            string actionName, string currentPagePath, string? nextPagePath)
        {
            session.IsUserChangingDetails = false;
            await SaveSession(session, currentPagePath, nextPagePath);

            return RedirectToAction(actionName);
        }

        private async Task SaveSession(OrganisationSession session, string currentPagePath, string? nextPagePath)
        {
            ClearRestOfJourney(session, currentPagePath);

            session.Journey.AddIfNotExists(nextPagePath);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        }

        private static void ClearRestOfJourney(OrganisationSession session, string currentPagePath)
        {
            var index = session.Journey.IndexOf(currentPagePath);

            // this also cover if current page not found (index = -1) then it clears all pages
            session.Journey = session.Journey.Take(index + 1).ToList();
        }
    }
}
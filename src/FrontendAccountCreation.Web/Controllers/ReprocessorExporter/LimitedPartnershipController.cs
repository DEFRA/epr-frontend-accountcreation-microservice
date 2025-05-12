using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using FrontendAccountCreation.Web.ViewModels;

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

        [HttpGet]
        [Route(PagePath.PartnershipType)]
        public async Task<IActionResult> PartnershipType()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.PartnershipType);

            return View();
        }

        [HttpPost]
        [Route(PagePath.PartnershipType)]
        public async Task<IActionResult> PartnershipType(PartnershipTypeRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session.ReExCompaniesHouseSession.IsPartnership = true;

            if (model.isLimitedPartnership == Core.Sessions.PartnershipType.LimitedPartnership)
            {
                session.ReExCompaniesHouseSession.Partnership = new Core.Sessions.ReEx.Partnership.ReExPartnership
                {
                    IsLimitedPartnership = true,
                    LimitedPartnership = new Core.Sessions.ReEx.Partnership.ReExLimitedPartnership()
                };

                return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipType), PagePath.PartnershipType, PagePath.LimitedPartnershipType);
            }
            else //TODO LimitedLiabilityPartnership
            //if (model.isLimitedPartnership == Core.Sessions.PartnershipType.LimitedLiabilityPartnership)
            { 
                session.ReExCompaniesHouseSession.Partnership = new Core.Sessions.ReEx.Partnership.ReExPartnership
                {
                    IsLimitedPartnership = true,
                    LimitedPartnership = new Core.Sessions.ReEx.Partnership.ReExLimitedPartnership()
                };

                return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipType), PagePath.PartnershipType, PagePath.LimitedPartnershipType);
            }
        }

        [HttpGet]
        [Route(PagePath.LimitedPartnershipType)]
        public async Task<IActionResult> LimitedPartnershipType()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.LimitedPartnershipType);
            return View();
        }

        [HttpPost]
        [Route(PagePath.LimitedPartnershipType)]
        public async Task<IActionResult> LimitedPartnershipType(LimitedPartnershipTypeRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session.ReExCompaniesHouseSession.IsPartnership = true;

            if (model.isIndividualPartners == true && model.isCompanyPartners == true)
            {
                //TODO 
            }
            else if (model.isIndividualPartners == true && model.isCompanyPartners == false)
            {
                //TODO 
            }
            else if (model.isIndividualPartners == false && model.isCompanyPartners == true)
            {
                //TODO 
            }

            session.ReExCompaniesHouseSession.Partnership = new Core.Sessions.ReEx.Partnership.ReExPartnership
            {
                IsLimitedPartnership = true,
                LimitedPartnership = new Core.Sessions.ReEx.Partnership.ReExLimitedPartnership()
            };

            return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipRole), PagePath.LimitedPartnershipType, PagePath.LimitedPartnershipRole);
        }

        [HttpPost]
        [Route(PagePath.LimitedPartnershipRole)]
        public async Task<IActionResult> LimitedPartnershipRole(LimitedPartnershipRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session.ReExCompaniesHouseSession.IsPartnership = true;

            session.ReExCompaniesHouseSession.Partnership = new Core.Sessions.ReEx.Partnership.ReExPartnership
            {
                IsLimitedPartnership = true,
                LimitedPartnership = new Core.Sessions.ReEx.Partnership.ReExLimitedPartnership()
            };

            return await SaveSessionAndRedirect(session, nameof(LimitedPartnershipRole), PagePath.LimitedPartnershipType, PagePath.LimitedPartnershipRole);
        }

        [HttpGet]
        [Route(PagePath.LimitedPartnershipRole)]
        public async Task<IActionResult> LimitedPartnershipRole()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.LimitedPartnershipType);
            return View();
        }

        private void SetBackLink(OrganisationSession session, string currentPagePath)
        {
            if (session.IsUserChangingDetails && currentPagePath != PagePath.CheckYourDetails)
            {
                ViewBag.BackLinkToDisplay = PagePath.CheckYourDetails;
            }
            else
            {
                ViewBag.BackLinkToDisplay = session.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
            }
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
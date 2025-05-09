using FrontendAccountCreation.Core.Sessions;
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
            return View();
        }

        [HttpGet]
        [Route(PagePath.LimitedPartnershipAddApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View();
        }

        [HttpPost]
        [Route(PagePath.AddAnApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson(AddApprovedPersonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.InviteUserOption == InviteUserOptions.BeAnApprovedPerson.ToString())
            {
                return RedirectToAction("YouAreApprovedPerson"); // need to re-visit with correct URL
            }

            if (model.InviteUserOption == InviteUserOptions.InviteAnotherPerson.ToString())
            {
                return RedirectToAction("TeamMemberRoleInOrganisation"); // need to re-visit with correct URL
            }

            // I-will-Invite-an-Approved-Person-Later
            return RedirectToAction("CheckYourDetails", "AccountCreation"); // need to re-visit with correct URL
        }
    }
}
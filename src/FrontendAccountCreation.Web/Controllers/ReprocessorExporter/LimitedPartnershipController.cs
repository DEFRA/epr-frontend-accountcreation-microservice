using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.LimitedPartnership;
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
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> OrganisationRole([FromQuery] Guid? id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            // show previously selected team member role
            if (id.HasValue)
            {
                int? index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index != null && index.GetValueOrDefault(-1) >= 0)
                {
                    OrganisationRoleViewModel viewModel = new OrganisationRoleViewModel
                    {
                        RoleInOrganisation = session.ReExCompaniesHouseSession.Partnership.LimitedPartnership.TeamMembers[index.Value]?.Role
                    };
                    return View(viewModel);
                }
            }

            return View();
        }

        [HttpPost]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> RoleInOrganisation(OrganisationRoleViewModel model)
        {
            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ReExCompaniesHouseSession companiesHouseSession = session.ReExCompaniesHouseSession ?? new();
            int? index = companiesHouseSession.TeamMembers?.FindIndex(0, x => x.Id.Equals(model?.Id));
            Guid queryStringId;
            bool isExistingMember = false;

            if (index != null && index.GetValueOrDefault(-1) >= 0)
            {
                // found existing team member, set their role
                queryStringId = model.Id.Value;
                isExistingMember = true;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].Role = model.RoleInOrganisation;

            }
            else
            {
                // add new team member
                queryStringId = Guid.NewGuid();

                List<LimitedPartnershipTeamMember> members = companiesHouseSession.TeamMembers ?? new();
                members.Add(new LimitedPartnershipTeamMember { Id = queryStringId, Role = model.RoleInOrganisation });
                companiesHouseSession.TeamMembers = members;
                session.ReExCompaniesHouseSession = companiesHouseSession;
            }

            session.IsUserChangingDetails = false;
            await SaveSession(session, PagePath.TeamMemberRoleInOrganisation, PagePath.TeamMemberDetails);

            if (isExistingMember)
            {
                // move to enter team member details: full name, email, telephone
                return RedirectToAction(nameof(TeamMembersCheckInvitationDetails));
            }
            else
            {
                // go back to check their invitation detials
                return RedirectToAction(nameof(TeamMemberDetails), new { id = queryStringId });
            }
        }

    }
}
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter
{
    [Route("re-ex/organisation")]
    public partial class ApprovedPersonController : Controller
    {
        private readonly ISessionManager<OrganisationSession> _sessionManager;

        public ApprovedPersonController(ISessionManager<OrganisationSession> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [HttpGet]
        [Route(ReExPagePath.AddAnApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View();
        }

        [HttpPost]
        [Route(ReExPagePath.AddAnApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson(AddApprovedPersonViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.InviteUserOption == InviteUserOptions.IWillInviteAnotherApprovedPerson.ToString())
            {
                return RedirectToAction(nameof(TeamMemberRoleInOrganisation));
            }
            else // I-will-Invite-an-Approved-Person-Later
            {
                return RedirectToAction("CheckYourDetails", "AccountCreation"); // need to re-visit with correct URL
            }
        }

        [HttpGet]
        [Route(ReExPagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation([FromQuery] Guid? id)
        {
            OrganisationSession session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();

            SetBackLink(session, ReExPagePath.TeamMemberRoleInOrganisation);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            // show previously selected team member role
            if (id.HasValue)
            {
                int? index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index != null && index.GetValueOrDefault(-1) >= 0)
                {
                    TeamMemberRoleInOrganisationViewModel viewModel = new TeamMemberRoleInOrganisationViewModel
                    {
                        RoleInOrganisation = session.ReExCompaniesHouseSession.TeamMembers[index.Value]?.Role
                    };
                    return View(viewModel);
                }
            }

            return View();
        }

        [HttpPost]
        [Route(ReExPagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation(TeamMemberRoleInOrganisationViewModel model)
        {
            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();
            if (!ModelState.IsValid)
            {
                SetBackLink(session, ReExPagePath.TeamMemberRoleInOrganisation);
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

                List<ReExCompanyTeamMember> members = companiesHouseSession.TeamMembers ?? new();
                members.Add(new ReExCompanyTeamMember { Id = queryStringId, Role = model.RoleInOrganisation });
                companiesHouseSession.TeamMembers = members;
                session.ReExCompaniesHouseSession = companiesHouseSession;

            }

            session.IsUserChangingDetails = false;
            await SaveSession(session, ReExPagePath.TeamMemberRoleInOrganisation, ReExPagePath.TeamMemberDetails);

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

        [HttpGet]
        [Route(ReExPagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMemberDetails([FromQuery] Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();
            SetBackLink(session, ReExPagePath.TeamMemberDetails);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            
            int? index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));

            if (index != null && index.GetValueOrDefault(-1) >= 0)
            {
                var viewModel = new TeamMemberViewModel
                {
                    Id = id,
                    FullName = session.ReExCompaniesHouseSession.TeamMembers[index.Value].FullName,
                    Telephone = session.ReExCompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber,
                    Email = session.ReExCompaniesHouseSession.TeamMembers[index.Value].Email
                };

                return View(viewModel);
            }

            return View();
        }

        [HttpPost]
        [Route(ReExPagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMemberDetails(TeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                ViewBag.BackLinkToDisplay = ReExPagePath.TeamMemberDetails;
                return View(model);
            }

            ReExCompaniesHouseSession companiesHouseSession = session.ReExCompaniesHouseSession ?? new();
            int? index = companiesHouseSession.TeamMembers?.FindIndex(0, x => x.Id.Equals(model.Id));

            if (index != null && index.GetValueOrDefault(-1) >= 0)
            {
                // found existing team member
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].FullName = model.FullName;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber = model.Telephone;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].Email = model.Email;
            }

            // go to check invitation details summary page for all team members
            return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails), ReExPagePath.TeamMemberDetails,
                ReExPagePath.TeamMembersCheckInvitationDetails);
        }

        /// <summary>
        /// Show team member details enetered so far
        /// </summary>
        /// <param name="id">Id of team member to remove</param>
        /// <returns></returns>
        [HttpGet]
        [Route(ReExPagePath.TeamMembersCheckInvitationDetails)]
        public async Task<IActionResult> TeamMembersCheckInvitationDetails([FromQuery] Guid? id)
        {
            OrganisationSession session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

            // if id is supplied, remove the team member
            if (id.HasValue)
            {
                int? index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index != null && index.GetValueOrDefault(-1) >= 0)
                {
                    session.ReExCompaniesHouseSession.TeamMembers.RemoveAt(index.Value);
                }
            }

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(session.ReExCompaniesHouseSession?.TeamMembers?.Where(x => !string.IsNullOrWhiteSpace(x.FullName)).ToList());

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

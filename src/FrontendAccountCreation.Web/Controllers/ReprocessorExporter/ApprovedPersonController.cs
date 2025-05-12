using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter
{
    [Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
    [Route("re-ex/organisation")]
    public partial class ApprovedPersonController : Controller
    {
        private readonly ISessionManager<OrganisationSession> _sessionManager;

        public ApprovedPersonController(ISessionManager<OrganisationSession> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [HttpGet]
        [Route(PagePath.AddAnApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.AddAnApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View();
        }

        [HttpPost]
        [Route(PagePath.AddAnApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.AddAnApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson(AddApprovedPersonViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
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
                return await SaveSessionAndRedirect(session, nameof(TeamMemberRoleInOrganisation),
                    PagePath.AddAnApprovedPerson, PagePath.TeamMemberRoleInOrganisation);
            }

            // I-will-Invite-an-Approved-Person-Later
            return RedirectToAction("CheckYourDetails", "AccountCreation"); // need to re-visit with correct URL
        }

        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation([FromQuery] Guid? id)
        {
            OrganisationSession session = await _sessionManager.GetSessionAsync(HttpContext.Session);

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
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation(TeamMemberRoleInOrganisationViewModel model)
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

                List<ReExCompanyTeamMember> members = companiesHouseSession.TeamMembers ?? new();
                members.Add(new ReExCompanyTeamMember { Id = queryStringId, Role = model.RoleInOrganisation });
                companiesHouseSession.TeamMembers = members;
                session.ReExCompaniesHouseSession = companiesHouseSession;
            }

            if (isExistingMember)
            {
                // move to enter team member details: full name, email, telephone
                return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails),
                    $"{PagePath.TeamMemberRoleInOrganisation}?id={queryStringId}",
                    PagePath.TeamMembersCheckInvitationDetails
                    );
            }
            else
            {
                //go back to check their invitation detials
            return await SaveSessionAndRedirect(session, nameof(TeamMemberDetails),
                $"{PagePath.TeamMemberRoleInOrganisation}",
                $"{PagePath.TeamMemberDetails}?id={queryStringId}",
                new { id = queryStringId });
            }
        }

        [HttpGet]
        [Route(PagePath.TeamMemberDetails)]
        [OrganisationJourneyAccess(PagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMemberDetails([FromQuery] Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToAction(nameof(TeamMemberRoleInOrganisation));
            }
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));

            if (index is >= 0)
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
        [Route(PagePath.TeamMemberDetails)]
        [OrganisationJourneyAccess(PagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMemberDetails(TeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var companiesHouseSession = session.ReExCompaniesHouseSession;
            var index = companiesHouseSession.TeamMembers?.FindIndex(0, x => x.Id.Equals(model.Id));

            if (index is >= 0)
            {
                // found existing team member
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].FullName = model.FullName;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber = model.Telephone;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].Email = model.Email;
            }

            // go to check invitation details summary page for all team members
            return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails), $"{PagePath.TeamMemberDetails}?id={model.Id}",
                PagePath.TeamMembersCheckInvitationDetails);
        }

        /// <summary>
        /// Show team member details enetered so far
        /// </summary>
        /// <param name="id">Id of team member to remove</param>
        /// <returns></returns>
        [HttpGet]
        [Route(PagePath.TeamMembersCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.TeamMembersCheckInvitationDetails)]
        public async Task<IActionResult> TeamMembersCheckInvitationDetails([FromQuery] Guid? id)
        {
            OrganisationSession session = await _sessionManager.GetSessionAsync(HttpContext.Session);

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

        [HttpGet]
        [Route(PagePath.AddNotApprovedPerson)]
        public async Task<IActionResult> AddNotApprovedPerson()
        {
            
            return View();
        }

        [HttpPost]
        [Route(PagePath.AddNotApprovedPerson)]
        public async Task<IActionResult> AddNotApprovedPerson(AddNotApprovedPersonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return model.InviteUserOption == InviteUserOptions.InviteAnotherPerson.ToString()
                ? RedirectToAction(nameof(TeamMemberRoleInOrganisation))
                : RedirectToAction("CheckYourDetails", "AccountCreation");
        }

        private async Task<RedirectToActionResult> SaveSessionAndRedirect(OrganisationSession session,
            string actionName, string currentPagePath, string? nextPagePath, object? routeValues = null)
        {
            session.IsUserChangingDetails = false;
            await SaveSession(session, currentPagePath, nextPagePath);

            return RedirectToAction(actionName, routeValues);
        }

        private async Task SaveSession(OrganisationSession session, string currentPagePath, string? nextPagePath)
        {
            ClearRestOfJourney(session, currentPagePath);
            if (nextPagePath == PagePath.TeamMembersCheckInvitationDetails)
            {
                session.Journey.AddIfNotExists(PagePath.TeamMemberDetails);
            }
            session.Journey.AddIfNotExists(nextPagePath);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        }

        private static void ClearRestOfJourney(OrganisationSession session, string currentPagePath)
        {
            var index = session.Journey.FindIndex(x => x.Contains(currentPagePath.Split("?")[0]));

            // this also cover if current page not found (index = -1) then it clears all pages
            session.Journey = session.Journey.Take(index + 1).ToList();
        }
    }
}
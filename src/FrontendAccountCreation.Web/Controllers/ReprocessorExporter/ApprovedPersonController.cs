using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter
{
    [Route("re-ex/organisation")]
    public partial class ApprovedPersonController : Controller
    {
        private readonly ISessionManager<OrganisationSession> _sessionManager;
        private readonly ExternalUrlsOptions _urlOptions;

        public ApprovedPersonController(ISessionManager<OrganisationSession> sessionManager,
        IOptions<ExternalUrlsOptions> urlOptions)
        {
            _sessionManager = sessionManager;
            _urlOptions = urlOptions.Value;
        }

        [HttpGet]
        [Route(PagePath.AddAnApprovedPerson)]
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

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (model.InviteUserOption == InviteUserOptions.BeAnApprovedPerson.ToString())
            {
                return await SaveSessionAndRedirect(session, nameof(YouAreApprovedPerson), PagePath.AddAnApprovedPerson, PagePath.YouAreApprovedPerson);
            }
            else if (model.InviteUserOption == InviteUserOptions.InviteAnotherPerson.ToString())
            {
                return await SaveSessionAndRedirect(session, nameof(TeamMemberRoleInOrganisation), PagePath.AddAnApprovedPerson, PagePath.TeamMemberRoleInOrganisation);
                return RedirectToAction(nameof(TeamMemberRoleInOrganisation));
            }

            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.AddApprovedPerson, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
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

            session.IsUserChangingDetails = false;
            await SaveSession(session, PagePath.TeamMemberRoleInOrganisation, PagePath.TeamMemberDetails);

            if (isExistingMember)
            {
                return RedirectToAction(nameof(TeamMembersCheckInvitationDetails));
            }
            else
            {
                return RedirectToAction(nameof(TeamMemberDetails), new { id = queryStringId });
            }
        }

        [HttpGet]
        [Route(PagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMemberDetails([FromQuery] Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            
            var index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));

            if (index is >= 0)
            {
                var viewModel = new TeamMemberViewModel
                {
                    Id = id,
                    FirstName = session.ReExCompaniesHouseSession.TeamMembers[index.Value].FirstName,
                    LastName = session.ReExCompaniesHouseSession.TeamMembers[index.Value].LastName,
                    Telephone = session.ReExCompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber,
                    Email = session.ReExCompaniesHouseSession.TeamMembers[index.Value].Email
                };

                return View(viewModel);
            }

            return View();
        }

        [HttpPost]
        [Route(PagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMemberDetails(TeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var index = session.ReExCompaniesHouseSession.TeamMembers?.FindIndex(0, x => x.Id.Equals(model.Id));

            if (index is >= 0)
            {
                // found existing team member
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].FirstName = model.FirstName;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].LastName = model.LastName;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber = model.Telephone;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].Email = model.Email;
            }

            // go to check invitation details summary page for all team members
            return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails), PagePath.TeamMemberDetails,
                PagePath.TeamMembersCheckInvitationDetails);
        }

        /// <summary>
        /// Show team member details enetered so far
        /// </summary>
        /// <param name="id">Id of team member to remove</param>
        /// <returns></returns>
        [HttpGet]
        [Route(PagePath.TeamMembersCheckInvitationDetails)]
        public async Task<IActionResult> TeamMembersCheckInvitationDetails([FromQuery] Guid? id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

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

            return View(session.ReExCompaniesHouseSession?.TeamMembers?.Where(x => !string.IsNullOrWhiteSpace(x.FirstName)).ToList());
        }

        [HttpGet]
        [Route(PagePath.YouAreApprovedPerson)]
        public async Task<IActionResult> YouAreApprovedPerson()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            return View();
        }

		[HttpPost]
		[Route(PagePath.YouAreApprovedPerson)]
		public async Task<IActionResult> YouAreApprovedPerson(bool inviteApprovedPerson)
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
			return View();
		}

		[HttpGet]
        [Route(PagePath.CheckYourDetails)]
	    public async Task<IActionResult> CheckYourDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            ViewBag.MakeChangesToYourLimitedCompanyLink = _urlOptions.MakeChangesToYourLimitedCompany;

            var viewModel = new ReExCheckYourDetailsViewModel
            {
                IsCompaniesHouseFlow = session.IsCompaniesHouseFlow,
                IsRegisteredAsCharity = session.IsTheOrganisationCharity,
                OrganisationType = session.OrganisationType,
                IsTradingNameDifferent = session.IsTradingNameDifferent,
                Nation = session.UkNation
            };
            if (viewModel.IsCompaniesHouseFlow)
            {
                viewModel.BusinessAddress = session.ReExCompaniesHouseSession.Company.BusinessAddress;
                viewModel.CompanyName = session.ReExCompaniesHouseSession?.Company.Name;
                viewModel.CompaniesHouseNumber = session.ReExCompaniesHouseSession?.Company.CompaniesHouseNumber;
                viewModel.RoleInOrganisation = session.ReExCompaniesHouseSession?.RoleInOrganisation;
            }
            if (session.ReExManualInputSession != null)
            {
                viewModel.TradingName = session.ReExManualInputSession.TradingName;
            }
            viewModel.reExCompanyTeamMembers = session.ReExCompaniesHouseSession?.TeamMembers;
            _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(viewModel);
        }

        private async Task<RedirectToActionResult> SaveSessionAndRedirect(OrganisationSession session,
            string actionName, string currentPagePath, string? nextPagePath, string? controllerName = null)
        {
            session.IsUserChangingDetails = false;
            await SaveSession(session, currentPagePath, nextPagePath);

            return !string.IsNullOrWhiteSpace(controllerName) ? RedirectToAction(actionName, controllerName) : RedirectToAction(actionName);
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

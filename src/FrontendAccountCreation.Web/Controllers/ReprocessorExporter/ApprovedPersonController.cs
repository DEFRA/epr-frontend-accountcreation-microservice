using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter
{
	[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
	[Route("re-ex/organisation")]
    public partial class ApprovedPersonController : ControllerBase<OrganisationSession>
	{
		private readonly ISessionManager<OrganisationSession> _sessionManager;
		private readonly ExternalUrlsOptions _urlOptions;

        public ApprovedPersonController(
            ISessionManager<OrganisationSession> sessionManager,
            IOptions<ExternalUrlsOptions> urlOptions) : base(sessionManager)
		{
			_sessionManager = sessionManager;
			_urlOptions = urlOptions.Value;
		}

		[HttpGet]
		[Route(PagePath.AddAnApprovedPerson)]
		[OrganisationJourneyAccess(PagePath.AddAnApprovedPerson)]
		public async Task<IActionResult> AddApprovedPerson()
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			SetBackLink(session, PagePath.AddAnApprovedPerson);
			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

			if (session.IsOrganisationAPartnership == true)
			{
				return session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson
					? View("InEligibleAddNotApprovedPerson")
					: View("LimitedPartnershipAddApprovedPerson");
			}

			return session.ReExCompaniesHouseSession?.IsInEligibleToBeApprovedPerson == true ? View("AddNotApprovedPerson") : View();
		}

		[HttpPost]
		[Route(PagePath.AddAnApprovedPerson)]
		[OrganisationJourneyAccess(PagePath.AddAnApprovedPerson)]
		public async Task<IActionResult> AddApprovedPerson(AddApprovedPersonViewModel model)
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

			if (!ModelState.IsValid)
			{
				if (session.IsOrganisationAPartnership == true)
				{
					return session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson
						? View("InEligibleAddNotApprovedPerson", model)
						: View("LimitedPartnershipAddApprovedPerson", model);
				}

				return session.ReExCompaniesHouseSession?.IsInEligibleToBeApprovedPerson == true ? View("AddNotApprovedPerson", model) : View(model);
			}

			if (model.InviteUserOption == InviteUserOptions.BeAnApprovedPerson.ToString())
			{
				return await SaveSessionAndRedirect(session, nameof(YouAreApprovedPerson), PagePath.AddAnApprovedPerson, PagePath.YouAreApprovedPerson);
			}

			if (model.InviteUserOption == InviteUserOptions.InviteAnotherPerson.ToString())
			{
				return await SaveSessionAndRedirect(session, nameof(TeamMemberRoleInOrganisation),
					PagePath.AddAnApprovedPerson, PagePath.TeamMemberRoleInOrganisation);
			}

			return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.AddAnApprovedPerson, PagePath.CheckYourDetails);
		}

		[HttpGet]
		[Route(PagePath.TeamMemberRoleInOrganisation)]
		[OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
		public async Task<IActionResult> TeamMemberRoleInOrganisation([FromQuery] Guid? id)
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);

			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

			var viewModel = new TeamMemberRoleInOrganisationViewModel();
			if (id.HasValue)
			{
				var index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
				if (index is >= 0)
				{
					viewModel.RoleInOrganisation = session.ReExCompaniesHouseSession.TeamMembers[index.Value]?.Role;
					return session.IsOrganisationAPartnership == true
						? View("ApprovedPersonPartnershipRole", viewModel)
						: View(viewModel);
				}
			}

			return session.IsOrganisationAPartnership == true
				? View("ApprovedPersonPartnershipRole", viewModel)
				: View(viewModel);
		}

		[HttpPost]
		[Route(PagePath.TeamMemberRoleInOrganisation)]
		[OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
		public async Task<IActionResult> TeamMemberRoleInOrganisation(TeamMemberRoleInOrganisationViewModel model)
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			if (!ModelState.IsValid)
			{
				return session.IsOrganisationAPartnership == true
					? View("ApprovedPersonPartnershipRole", model)
					: View(model);
			}

			var companiesHouseSession = session.ReExCompaniesHouseSession ?? new();
			var members = companiesHouseSession.TeamMembers ?? new();
			var index = members.FindIndex(0, x => x.Id.Equals(model?.Id));
			var isExistingMember = index >= 0;
			var queryStringId = model?.Id ?? Guid.NewGuid();

			if (model.RoleInOrganisation == ReExTeamMemberRole.None)
			{
				if (isExistingMember)
				{
					members.RemoveAt(index);
					companiesHouseSession.TeamMembers = members;
					session.ReExCompaniesHouseSession = companiesHouseSession;

					return await SaveSessionAndRedirect(
						session,
						nameof(CheckYourDetails),
						$"{PagePath.TeamMemberRoleInOrganisation}?id={queryStringId}",
						PagePath.CheckYourDetails);
				}

				return await SaveSessionAndRedirect(
					session,
					nameof(PersonCanNotBeInvited),
					$"{PagePath.TeamMemberRoleInOrganisation}?id={queryStringId}",
					PagePath.ApprovedPersonPartnershipCanNotBeInvited);
			}

			if (isExistingMember)
			{
				members[index].Role = model.RoleInOrganisation;
			}
			else
			{
				members.Add(new ReExCompanyTeamMember
				{
					Id = queryStringId,
					Role = model.RoleInOrganisation
				});
			}

			companiesHouseSession.TeamMembers = members;
			session.ReExCompaniesHouseSession = companiesHouseSession;

			if (isExistingMember)
			{
				return await SaveSessionAndRedirect(
					session,
					nameof(TeamMembersCheckInvitationDetails),
					$"{PagePath.TeamMemberRoleInOrganisation}?id={queryStringId}",
					PagePath.TeamMembersCheckInvitationDetails);
			}

			companiesHouseSession.TeamMembers = members;
			session.ReExCompaniesHouseSession = companiesHouseSession;

			if (isExistingMember)
			{
				return await SaveSessionAndRedirect(
					session,
					nameof(TeamMembersCheckInvitationDetails),
					$"{PagePath.TeamMemberRoleInOrganisation}?id={queryStringId}",
					PagePath.TeamMembersCheckInvitationDetails);
			}

			return await SaveSessionAndRedirect(
				session,
				nameof(TeamMemberDetails),
				$"{PagePath.TeamMemberRoleInOrganisation}",
				$"{PagePath.TeamMemberDetails}",
				null,
				new { id = queryStringId });
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
			SetBackLink(session, PagePath.TeamMemberDetails);

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
		[OrganisationJourneyAccess(PagePath.TeamMemberDetails)]
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
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			SetBackLink(session, PagePath.TeamMembersCheckInvitationDetails);
			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

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
		[OrganisationJourneyAccess(PagePath.YouAreApprovedPerson)]
		public async Task<IActionResult> YouAreApprovedPerson()
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			SetBackLink(session, PagePath.YouAreApprovedPerson);
			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

			var approvedPersonViewModel = new ApprovedPersonViewModel
			{
				IsLimitedLiabilityPartnership = session.ReExCompaniesHouseSession.Partnership.IsLimitedLiabilityPartnership,
				IsLimitedPartnership = session.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership
			};

			return session.IsOrganisationAPartnership == true
				? View(approvedPersonViewModel)
				: View();
		}

		[HttpPost]
		[Route(PagePath.YouAreApprovedPerson)]
		[OrganisationJourneyAccess(PagePath.YouAreApprovedPerson)]
		public async Task<IActionResult> YouAreApprovedPerson(bool inviteApprovedPerson)
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			SetBackLink(session, PagePath.YouAreApprovedPerson);

			var nextPage = inviteApprovedPerson
				? PagePath.TeamMemberRoleInOrganisation
				: PagePath.CheckYourDetails;

			var nextAction = inviteApprovedPerson
				? nameof(TeamMemberRoleInOrganisation)
				: nameof(CheckYourDetails);

			return await SaveSessionAndRedirect(session, nextAction, PagePath.YouAreApprovedPerson, nextPage);
		}

		[HttpGet]
		[Route(PagePath.MemberPartnership)]
		[OrganisationJourneyAccess(PagePath.MemberPartnership)]
		public async Task<IActionResult> MemberPartnership()
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			SetBackLink(session, PagePath.MemberPartnership);
			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

			var viewModel = new IsMemberPartnershipViewModel();
			return View(viewModel);
		}

		[HttpPost]
		[Route(PagePath.MemberPartnership)]
		[OrganisationJourneyAccess(PagePath.MemberPartnership)]
		public async Task<IActionResult> MemberPartnership(IsMemberPartnershipViewModel model)
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			if (!ModelState.IsValid)
			{
				SetBackLink(session, PagePath.MemberPartnership);
				return View(model);
			}

			if (model.IsMemberPartnership == YesNoAnswer.Yes)
			{
				return await SaveSessionAndRedirect(session, nameof(PartnerDetails), PagePath.MemberPartnership, PagePath.PartnerDetails);
			}

			return await SaveSessionAndRedirect(session, nameof(CanNotInviteThisPerson), PagePath.MemberPartnership, PagePath.CanNotInviteThisPerson);
		}

        [HttpGet]
        [Route(PagePath.PartnerDetails)]
        [OrganisationJourneyAccess(PagePath.PartnerDetails)]
        public async Task<IActionResult> PartnerDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.PartnerDetails);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View();
        }

        [HttpGet]
        [Route(PagePath.CanNotInviteThisPerson)]
        [OrganisationJourneyAccess(PagePath.CanNotInviteThisPerson)]
        public async Task<IActionResult> CanNotInviteThisPerson()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.CanNotInviteThisPerson);
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

		[HttpGet]
		[Route(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
		[OrganisationJourneyAccess(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
		public async Task<IActionResult> PersonCanNotBeInvited([FromQuery] Guid id)
		{
			var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
			SetBackLink(session, PagePath.ApprovedPersonPartnershipCanNotBeInvited);
			await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

			return View(new LimitedPartnershipPersonCanNotBeInvitedViewModel { Id = id });
		}

		[HttpPost]
		[Route(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
		[OrganisationJourneyAccess(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
		public IActionResult PersonCanNotBeInvited(LimitedPartnershipPersonCanNotBeInvitedViewModel model)
		{
			return RedirectToAction("CheckYourDetails", "AccountCreation");
		}
	}
}
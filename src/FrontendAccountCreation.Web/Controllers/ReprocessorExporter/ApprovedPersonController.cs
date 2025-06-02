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

            var model = new AddApprovedPersonViewModel
            {
                IsOrganisationAPartnership = session.IsOrganisationAPartnership,
                IsInEligibleToBeApprovedPerson =
                    session.ReExCompaniesHouseSession?.IsInEligibleToBeApprovedPerson ?? false,
				IsLimitedPartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership ?? false,
				IsLimitedLiablePartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false
            };

            return View(model); 
        }

        [HttpPost]
        [Route(PagePath.AddAnApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.AddAnApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson(AddApprovedPersonViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.AddAnApprovedPerson);
                model.IsOrganisationAPartnership = session.IsOrganisationAPartnership;
                model.IsLimitedPartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership ?? false;
                model.IsLimitedLiablePartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false;
                model.IsInEligibleToBeApprovedPerson = session.ReExCompaniesHouseSession?.IsInEligibleToBeApprovedPerson ?? false;
                return View(model);
            }

            if (model.InviteUserOption == InviteUserOptions.BeAnApprovedPerson.ToString())
            {
                session.IsApprovedUser = true;
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
                SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);
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
                SetBackLink(session, PagePath.TeamMemberDetails);
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

        [HttpPost]
        [Route(PagePath.TeamMembersCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.TeamMembersCheckInvitationDetails)]
        public async Task<IActionResult> TeamMembersCheckInvitationDetailsPost()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.TeamMembersCheckInvitationDetails, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.YouAreApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.YouAreApprovedPerson)]
        public async Task<IActionResult> YouAreApprovedPerson()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.YouAreApprovedPerson);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            bool isPartnership = session.IsOrganisationAPartnership ?? false;

            var approvedPersonViewModel = new ApprovedPersonViewModel
            {
                IsLimitedLiabilityPartnership = isPartnership && (session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false),
                IsLimitedPartnership = isPartnership && (session.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership ?? false)
            };

            return View(approvedPersonViewModel);
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
                //return await SaveSessionAndRedirect(session, "PartnerDetails", PagePath.MemberPartnership, PagePath.PartnerDetails);
                if (model.IsMemberPartnership == YesNoAnswer.Yes)
                {
                    var partnershipId = Guid.NewGuid();
                    return await SaveSessionAndRedirect(
                        session,
                        actionName: "PartnerDetails",
                        currentPagePath: PagePath.MemberPartnership,
                        nextPagePath: PagePath.PartnerDetails,
                        controllerName: "ApprovedPersonController",
                        routeValues: new { id = partnershipId }
                    );
                }
            }

            return await SaveSessionAndRedirect(session, "CanNotInviteThisPerson", PagePath.MemberPartnership, PagePath.CanNotInviteThisPerson);
        }

        [HttpGet]
        [Route(PagePath.PartnerDetails)]
        [OrganisationJourneyAccess(PagePath.PartnerDetails)]
        public async Task<IActionResult> PartnerDetails([FromQuery] Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToAction(nameof(MemberPartnership));
            }

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.PartnerDetails);

            if (session.ReExCompaniesHouseSession.TeamMembers == null)
            {
                session.ReExCompaniesHouseSession.TeamMembers = new List<ReExCompanyTeamMember>();
            }

            var teamMembers = session.ReExCompaniesHouseSession.TeamMembers;
            var member = teamMembers.FirstOrDefault(x => x.Id == id);

            if (member == null)
            {
                member = new ReExCompanyTeamMember { Id = id };
                teamMembers.Add(member);
                await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            }

            var viewModel = new TeamMemberViewModel
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Telephone = member.TelephoneNumber,
                Email = member.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.PartnerDetails)]
        [OrganisationJourneyAccess(PagePath.PartnerDetails)]
        public async Task<IActionResult> PartnerDetails(TeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var index = session.ReExCompaniesHouseSession.TeamMembers?.FindIndex(0, x => x.Id.Equals(model.Id));

            if (index is >= 0)
            {
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].FirstName = model.FirstName;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].LastName = model.LastName;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber = model.Telephone;
                session.ReExCompaniesHouseSession.TeamMembers[index.Value].Email = model.Email;
            }

            return await SaveSessionAndRedirect(session, "CheckPartnerInvitation", $"{PagePath.CheckPartnerInvitation}?id={model.Id}",
                PagePath.TeamMembersCheckInvitationDetails);
        }

        [HttpGet]
        [Route(PagePath.CheckYourDetails)]
        [OrganisationJourneyAccess(PagePath.CheckYourDetails)]
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
                viewModel.IsOrganisationAPartnership = session.IsOrganisationAPartnership ?? false;
                viewModel.LimitedPartnershipPartners = session.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners;
                viewModel.IsLimitedLiabilityPartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false;

            }
            if (session.ReExManualInputSession != null)
            {
                viewModel.TradingName = session.ReExManualInputSession.TradingName;
            }
            viewModel.reExCompanyTeamMembers = session.ReExCompaniesHouseSession?.TeamMembers;
            _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.CheckYourDetails)]
        [OrganisationJourneyAccess(PagePath.CheckYourDetails)]
        public async Task<IActionResult> CheckYourDetailsPost()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            return await SaveSessionAndRedirect(session, nameof(OrganisationController.Declaration), PagePath.CheckYourDetails, PagePath.Declaration, nameof(OrganisationController).Replace("Controller", ""));
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
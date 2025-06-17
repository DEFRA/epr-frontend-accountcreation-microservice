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
    public class ApprovedPersonController : ControllerBase<OrganisationSession>
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
                IsLimitedLiablePartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false,
                IsIndividualInCharge = session.IsIndividualInCharge ?? false,
                IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader
            };

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.AddAnApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.AddAnApprovedPerson)]
        public async Task<IActionResult> AddApprovedPerson(AddApprovedPersonViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            model.IsIndividualInCharge = session.IsIndividualInCharge ?? false;
            model.IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader;

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.AddAnApprovedPerson);
                model.IsOrganisationAPartnership = session.IsOrganisationAPartnership;
                model.IsLimitedPartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership ?? false;
                model.IsLimitedLiablePartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false;
                model.IsInEligibleToBeApprovedPerson = session.ReExCompaniesHouseSession?.IsInEligibleToBeApprovedPerson ?? false;
                var errorMessage = model.IsSoleTrader ? "AddNotApprovedPerson.SoleTrader.ErrorMessage" : "AddAnApprovedPerson.OptionError";
                ModelState.ClearValidationState(nameof(model.InviteUserOption));
                ModelState.AddModelError(nameof(model.InviteUserOption), errorMessage);
                return View(model);
            }

            model.IsIndividualInCharge = session.IsIndividualInCharge ?? false;
            model.IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader;

            if (model.InviteUserOption == InviteUserOptions.BeAnApprovedPerson.ToString())
            {
                session.IsApprovedUser = true;
                return await SaveSessionAndRedirect(session, nameof(YouAreApprovedPerson), PagePath.AddAnApprovedPerson, PagePath.YouAreApprovedPerson);
            }

            if (model.InviteUserOption == InviteUserOptions.InviteAnotherPerson.ToString())
            {
                if(model.IsSoleTrader && !model.IsIndividualInCharge)
                {
                    return await SaveSessionAndRedirect(session, nameof(SoleTraderTeamMemberDetails),
                    PagePath.AddAnApprovedPerson, PagePath.SoleTraderTeamMemberDetails);
                }
                return await SaveSessionAndRedirect(session, nameof(TeamMemberRoleInOrganisation),
                    PagePath.AddAnApprovedPerson, PagePath.TeamMemberRoleInOrganisation);
            }

            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.AddAnApprovedPerson, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var viewModel = new TeamMemberRoleInOrganisationViewModel();

            var id = GetFocusId();
            if (id.HasValue)
            {
                var index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index is >= 0)
                {
                    viewModel.Id = id;
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
            bool isExistingMember = false;
            if (index >= 0)
            {
                // check the email, but any field other than Id will do
                isExistingMember = members[index].Email?.Length > 0;
            }
            var queryStringId = model?.Id ?? Guid.NewGuid();

            if (model.RoleInOrganisation == ReExTeamMemberRole.None)
            {
                if (isExistingMember)
                {
                    members.RemoveAt(index);
                    companiesHouseSession.TeamMembers = members;
                    session.ReExCompaniesHouseSession = companiesHouseSession;

                    SetFocusId(queryStringId);
                    return await SaveSessionAndRedirect(
                        session,
                        nameof(CheckYourDetails),
                        PagePath.TeamMemberRoleInOrganisation,
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
                    PagePath.TeamMemberRoleInOrganisation,
                    PagePath.TeamMembersCheckInvitationDetails);
            }

            SetFocusId(queryStringId);
            return await SaveSessionAndRedirect(
                session,
                nameof(TeamMemberDetails),
                PagePath.TeamMemberRoleInOrganisation,
                PagePath.TeamMemberDetails);
        }

        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisationAdd)]
        [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisationAdd()
        {
            DeleteFocusId();

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.TeamMemberRoleInOrganisation),
                PagePath.TeamMemberRoleInOrganisation, null);
        }

        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisationEdit)]
        public async Task<IActionResult> TeamMemberRoleInOrganisationEdit([FromQuery] Guid id)
        {
            SetFocusId(id);

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.TeamMemberRoleInOrganisation),
                PagePath.TeamMemberRoleInOrganisation, null);
        }

        [HttpGet]
        [Route(PagePath.SoleTraderTeamMemberDetails)]
        [OrganisationJourneyAccess(PagePath.SoleTraderTeamMemberDetails)]
        public async Task<IActionResult> SoleTraderTeamMemberDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.SoleTraderTeamMemberDetails);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var viewModel = new SoleTraderTeamMemberViewModel();

            if (session.ReExManualInputSession?.TeamMember != null)
            {
                viewModel.FirstName = session.ReExManualInputSession.TeamMember.FirstName;
                viewModel.LastName = session.ReExManualInputSession.TeamMember.LastName;
                viewModel.Telephone = session.ReExManualInputSession.TeamMember.TelephoneNumber;
                viewModel.Email = session.ReExManualInputSession.TeamMember.Email;
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.SoleTraderTeamMemberDetails)]
        [OrganisationJourneyAccess(PagePath.SoleTraderTeamMemberDetails)]
        public async Task<IActionResult> SoleTraderTeamMemberDetails(SoleTraderTeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                SetBackLink(session!, PagePath.SoleTraderTeamMemberDetails);
                return View(model);
            }

            var teamMember = session!.ReExManualInputSession!.TeamMember ??= new ReExCompanyTeamMember();

            teamMember.FirstName = model.FirstName;
            teamMember.LastName = model.LastName;
            teamMember.TelephoneNumber = model.Telephone;
            teamMember.Email = model.Email;
            teamMember.Role = ReExTeamMemberRole.SoleTrader;

            return await SaveSessionAndRedirect(session, nameof(SoleTraderTeamMemberCheckInvitationDetails), PagePath.SoleTraderTeamMemberDetails,
                PagePath.SoleTraderTeamMemberCheckInvitationDetails);
        }

        [HttpGet]
        [Route(PagePath.SoleTraderTeamMemberCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.SoleTraderTeamMemberCheckInvitationDetails)]
        public async Task<IActionResult> SoleTraderTeamMemberCheckInvitationDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.SoleTraderTeamMemberCheckInvitationDetails);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(session.ReExManualInputSession?.TeamMember);
        }

        [HttpPost]
        [Route(PagePath.SoleTraderTeamMemberCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.SoleTraderTeamMemberCheckInvitationDetails)]
        public async Task<IActionResult> SoleTraderTeamMemberCheckInvitationDetailsPost()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.SoleTraderTeamMemberCheckInvitationDetails, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.SoleTraderTeamMemberCheckInvitationDetailsDelete)]
        public async Task<IActionResult> SoleTraderTeamMemberCheckInvitationDetailsDelete()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session.ReExManualInputSession.TeamMember = null;

            return await SaveSessionAndRedirect(session, nameof(SoleTraderTeamMemberCheckInvitationDetails),
                PagePath.SoleTraderTeamMemberCheckInvitationDetails, null);
        }

        [HttpGet]
        [Route(PagePath.TeamMemberDetails)]
        [OrganisationJourneyAccess(PagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMemberDetails()
        {
            Guid? id = GetFocusId();
            if (id.HasValue)
            {
                SetFocusId(id.Value);
            }
            else
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
                    Id = id.Value,
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
            return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails), PagePath.TeamMemberDetails,
                PagePath.TeamMembersCheckInvitationDetails);
        }

        [HttpGet]
        [Route(PagePath.TeamMemberDetailsEdit)]
        public async Task<IActionResult> TeamMemberDetailsEdit([FromQuery] Guid id)
        {
            SetFocusId(id);

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.TeamMemberDetails),
                PagePath.TeamMemberDetails, null);
        }

        /// <summary>
        /// Show team member details entered so far
        /// </summary>
        /// <param name="id">Id of team member to remove</param>
        /// <returns></returns>
        [HttpGet]
        [Route(PagePath.TeamMembersCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.TeamMembersCheckInvitationDetails)]
        public async Task<IActionResult> TeamMembersCheckInvitationDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.TeamMembersCheckInvitationDetails);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(session.ReExCompaniesHouseSession?.TeamMembers?.Where(x => !string.IsNullOrWhiteSpace(x.FirstName)).ToList());
        }

        [HttpPost]
        [Route(PagePath.TeamMembersCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.TeamMembersCheckInvitationDetails)]
        public async Task<IActionResult> TeamMembersCheckInvitationDetailsPost(List<ReExCompanyTeamMember>? modelNotUsed)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.TeamMembersCheckInvitationDetails, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.TeamMembersCheckInvitationDetailsDelete)]
        public async Task<IActionResult> TeamMembersCheckInvitationDetailsDelete([FromQuery] Guid id)
        {
            DeleteFocusId();

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session?.ReExCompaniesHouseSession?.TeamMembers?.RemoveAll(x => x.Id == id);

            return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails),
                PagePath.TeamMembersCheckInvitationDetails, null);
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
        [Route(PagePath.YouAreApprovedPersonSoleTrader)]
        [OrganisationJourneyAccess(PagePath.YouAreApprovedPersonSoleTrader)]
        public async Task<IActionResult> YouAreApprovedPersonSoleTrader()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.YouAreApprovedPersonSoleTrader);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View();
        }

        [HttpGet]
        [Route(PagePath.SoleTraderContinue)]
        public async Task<IActionResult> SoleTraderContinue()
        {
            //to-do: will this mean going back will loop forward?
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.YouAreApprovedPersonSoleTrader);
            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.SoleTraderContinue,  PagePath.CheckYourDetails);
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
                return await SaveSessionAndRedirect(session, "PartnerDetails", PagePath.MemberPartnership, PagePath.PartnerDetails);
            }

            return await SaveSessionAndRedirect(session, "CanNotInviteThisPerson", PagePath.MemberPartnership, PagePath.CanNotInviteThisPerson);
        }

        [HttpGet]
        [Route(PagePath.MemberPartnershipAdd)]
        public async Task<IActionResult> MemberPartnershipAdd()
        {
            DeleteFocusId();

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(MemberPartnership),
                PagePath.YouAreApprovedPerson, PagePath.MemberPartnership);
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
                IsManualInputFlow = !session.IsCompaniesHouseFlow,
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
                viewModel.reExCompanyTeamMembers = session.ReExCompaniesHouseSession?.TeamMembers;
            }
            else if (viewModel.IsManualInputFlow)
            {
                viewModel.IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader;
                viewModel.ProducerType = session.ReExManualInputSession?.ProducerType;
                viewModel.BusinessAddress = session.ReExManualInputSession?.BusinessAddress;
                viewModel.TradingName = session.ReExManualInputSession?.TradingName;
                var teamMember = session.ReExManualInputSession?.TeamMember;
                viewModel.reExCompanyTeamMembers = new List<ReExCompanyTeamMember>();

                if (teamMember != null)
                {
                    viewModel.reExCompanyTeamMembers.Add(teamMember);
                }
            }
            
            _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.CheckYourDetails)]
        [OrganisationJourneyAccess(PagePath.CheckYourDetails)]
        public async Task<IActionResult> CheckYourDetailsPost()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            return await SaveSessionAndRedirect(session, 
                controllerName: nameof(OrganisationController), 
                actionName: nameof(OrganisationController.Declaration),
                currentPagePath: PagePath.CheckYourDetails, 
                nextPagePath: PagePath.Declaration);
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
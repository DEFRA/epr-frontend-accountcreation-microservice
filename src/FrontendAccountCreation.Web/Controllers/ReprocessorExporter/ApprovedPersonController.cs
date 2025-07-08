using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Extensions;
using FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;
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
        private const string ApprovedPersonErrorMessage = "AddAnApprovedPerson.OptionError";

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

            var id = GetFocusId();
            if (id.HasValue)
            {
                SetFocusId(id.Value);
            }

            var model = new AddApprovedPersonViewModel
            {
                InviteUserOption = session.InviteUserOption?.ToString(),
                IsOrganisationAPartnership = session.IsOrganisationAPartnership,
                IsInEligibleToBeApprovedPerson = !IsEligibleToBeApprovedPerson(session),
                IsLimitedPartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership ?? false,
                IsLimitedLiablePartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false,
                IsIndividualInCharge = session.IsIndividualInCharge ?? false,
                IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader,
                IsNonUk = session.IsUkMainAddress == false
            };

            return View(model);
        }

        private static bool IsEligibleToBeApprovedPerson(OrganisationSession session)
        {
            bool isEligibleToBeApprovedPerson = false;
            if (session.ReExCompaniesHouseSession != null)
            {
                isEligibleToBeApprovedPerson = !session.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson;
            }
            else if (session.ReExManualInputSession != null)
            {
                isEligibleToBeApprovedPerson = session.ReExManualInputSession.IsEligibleToBeApprovedPerson == true;
            }

            return isEligibleToBeApprovedPerson;
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
                model.IsInEligibleToBeApprovedPerson = !IsEligibleToBeApprovedPerson(session);
                model.IsNonUk = session.IsUkMainAddress == false;

                ModelState.ClearValidationState(nameof(model.InviteUserOption));
                ModelState.AddModelError(nameof(model.InviteUserOption), ApprovedPersonErrorMessage);
                return View(model);
            }

            model.IsIndividualInCharge = session.IsIndividualInCharge ?? false;
            model.IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader;
            session.InviteUserOption = session.InviteUserOption = model.InviteUserOption.ToEnumOrNull<InviteUserOptions>();

            switch (model.InviteUserOption)
            {
                case nameof(InviteUserOptions.BeAnApprovedPerson):
                    session.IsApprovedUser = true;
                    return await SaveSessionAndRedirect(session, nameof(YouAreApprovedPerson), PagePath.AddAnApprovedPerson, PagePath.YouAreApprovedPerson);

                case nameof(InviteUserOptions.InviteAnotherPerson):
                    string actionName, nextPagePath;

                    if (session is { IsOrganisationAPartnership: true, ReExCompaniesHouseSession.Partnership.IsLimitedLiabilityPartnership: true })
                    {
                        actionName = nameof(MemberPartnership);
                        nextPagePath = PagePath.MemberPartnership;
                    }
                    else if (session.IsCompaniesHouseFlow)
                    {
                        actionName = nameof(TeamMemberRoleInOrganisation);
                        nextPagePath = PagePath.TeamMemberRoleInOrganisation;
                    }
                    else
                    {
                        if (session.IsUkMainAddress is false)
                        {
                            return await SaveSessionAndRedirectToPage(
                                session,
                                nameof(ManageControlOrganisation),
                                PagePath.AddAnApprovedPerson,
                                PagePath.ManageControlOrganisation);
                        }
                        actionName = nameof(AreTheyIndividualInCharge);
                        nextPagePath = PagePath.IndividualIncharge;
                    }

                    return await SaveSessionAndRedirect(session, actionName, PagePath.AddAnApprovedPerson, nextPagePath);

                case nameof(InviteUserOptions.InviteLater):
                    return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.AddAnApprovedPerson, PagePath.CheckYourDetails);
            }

            var id = GetFocusId();
            if (id.HasValue)
            {
                SetFocusId(id.Value);
                if (model.IsSoleTrader && !model.IsIndividualInCharge)
                {
                    return await SaveSessionAndRedirect(session, nameof(NonCompaniesHouseTeamMemberDetails),
                    PagePath.AddAnApprovedPerson, PagePath.NonCompaniesHouseTeamMemberDetails);
                }
                return await SaveSessionAndRedirect(session, nameof(TeamMemberRoleInOrganisation),
                    PagePath.AddAnApprovedPerson, PagePath.TeamMemberRoleInOrganisation);
            }

            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.AddAnApprovedPerson, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.IndividualIncharge)]
        [OrganisationJourneyAccess(PagePath.IndividualIncharge)]
        public async Task<IActionResult> AreTheyIndividualInCharge(bool resetOptions = false)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.ManageControlOrganisation);

            YesNoAnswer? theyInCharge = null;
            if (session.AreTheyIndividualInCharge.HasValue && !resetOptions)
            {
                theyInCharge = session.AreTheyIndividualInCharge == true ? YesNoAnswer.Yes : YesNoAnswer.No;
            }

            return View(new TheyIndividualInChargeViewModel
            {
                AreTheyIndividualInCharge = resetOptions ? null : theyInCharge
            });
        }

        [HttpPost]
        [Route(PagePath.IndividualIncharge)]
        [OrganisationJourneyAccess(PagePath.IndividualIncharge)]
        public async Task<IActionResult> AreTheyIndividualInCharge(TheyIndividualInChargeViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.ManageControlOrganisation);
                return View(model);
            }

            session.AreTheyIndividualInCharge = model.AreTheyIndividualInCharge == YesNoAnswer.Yes;

            if (model.AreTheyIndividualInCharge.HasValue && model.AreTheyIndividualInCharge == YesNoAnswer.Yes)
            {
                return await SaveSessionAndRedirect(session,
                    nameof(NonCompaniesHouseTeamMemberDetails),
                    PagePath.IndividualIncharge,
                    PagePath.NonCompaniesHouseTeamMemberDetails);
            }
            else
            {
                return await SaveSessionAndRedirect(session,
                    nameof(PersonCanNotBeInvited),
                    PagePath.IndividualIncharge,
                    PagePath.ApprovedPersonPartnershipCanNotBeInvited);
            }
        }

        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        [OrganisationJourneyAccess(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            var isLimitedLiabilityPartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedLiabilityPartnership == true;
            var isLimitedPartnership = session.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership == true;

            var viewModel = new TeamMemberRoleInOrganisationViewModel();
            var llpViewModel = new IsMemberPartnershipViewModel();

            var id = GetFocusId();

            if (id.HasValue)
            {
                var index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index is >= 0)
                {
                    if (isLimitedLiabilityPartnership)
                    {
                        var memberRole = session.ReExCompaniesHouseSession.TeamMembers[index.Value].Role.ToString()
                            .ToEnumOrNull<ReExTeamMemberRole>();

                        llpViewModel.Id = id;
                        llpViewModel.IsMemberPartnership = memberRole == ReExTeamMemberRole.Member ? YesNoAnswer.Yes : YesNoAnswer.No;
                    }
                    else
                    {
                        viewModel.Id = id;
                        viewModel.RoleInOrganisation = session.ReExCompaniesHouseSession.TeamMembers[index.Value]?.Role;
                    }
                }
                SetFocusId(id.Value);
            }

            if (isLimitedPartnership)
            {
                return View("ApprovedPersonPartnershipRole", viewModel);
            }

            return isLimitedLiabilityPartnership ?
                View("MemberPartnership", llpViewModel) :
                View(viewModel);
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
        [Route(PagePath.TeamMemberRoleInOrganisationContinueWithoutInvitation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisationContinueWithoutInvitation()
        {
            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails),
                PagePath.TeamMemberRoleInOrganisation, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.NonCompaniesHouseTeamMemberDetails)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHouseTeamMemberDetails)]
        public async Task<IActionResult> NonCompaniesHouseTeamMemberDetails(Guid? id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            SetBackLink(session, PagePath.NonCompaniesHouseTeamMemberDetails);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var viewModel = new NonCompaniesHouseTeamMemberViewModel();

            if (id.HasValue)
            {
                var teamMember = session.ReExManualInputSession?.TeamMembers?
                    .Find(member => member.Id == id.Value);

                if (teamMember != null)
                {
                    viewModel = new NonCompaniesHouseTeamMemberViewModel
                    {
                        Id = teamMember.Id,
                        FirstName = teamMember.FirstName,
                        LastName = teamMember.LastName,
                        Telephone = teamMember.TelephoneNumber,
                        Email = teamMember.Email
                    };
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.NonCompaniesHouseTeamMemberDetails)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHouseTeamMemberDetails)]
        public async Task<IActionResult> NonCompaniesHouseTeamMemberDetails(NonCompaniesHouseTeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.NonCompaniesHouseTeamMemberDetails);
                return View(model);
            }

            var teamMembers = session.ReExManualInputSession!.TeamMembers ??= new List<ReExCompanyTeamMember>();
            var existingMember = teamMembers.Find(m => m.Id == model.Id);

            if (existingMember != null)
            {
                existingMember.FirstName = model.FirstName;
                existingMember.LastName = model.LastName;
                existingMember.TelephoneNumber = model.Telephone;
                existingMember.Email = model.Email;
            }
            else
            {
                teamMembers.Add(new ReExCompanyTeamMember
                {
                    Id = Guid.NewGuid(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    TelephoneNumber = model.Telephone,
                    Email = model.Email,
                });
            }

            return await SaveSessionAndRedirect(
                session,
                nameof(NonCompaniesHouseTeamMemberCheckInvitationDetails),
                PagePath.NonCompaniesHouseTeamMemberDetails,
                PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails);
        }

        [HttpGet]
        [Route(PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails)]
        public async Task<IActionResult> NonCompaniesHouseTeamMemberCheckInvitationDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var model = new NonCompaniesHouseTeamMemberCheckInvitationDetailsViewModel
            {
                TeamMembers = session.ReExManualInputSession?.TeamMembers,
                IsNonUk = session.IsUkMainAddress == false,
                IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader
            };

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails)]
        public async Task<IActionResult> NonCompaniesHouseTeamMemberCheckInvitationDetailsPost()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails, PagePath.CheckYourDetails);
        }

        [HttpPost]
        [Route(PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetailsDelete)]
        public async Task<IActionResult> NonCompaniesHouseTeamMemberCheckInvitationDetailsDelete(Guid? id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if (id.HasValue && session.ReExManualInputSession?.TeamMembers != null)
            {
                session.ReExManualInputSession.TeamMembers =
                    session.ReExManualInputSession.TeamMembers
                        .Where(tm => tm.Id != id.Value)
                        .ToList();
            }

            return await SaveSessionAndRedirect(
                session,
                nameof(NonCompaniesHouseTeamMemberCheckInvitationDetails),
                PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails,
                null
            );
        }

        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisationAddAnother)]
        [OrganisationJourneyAccess(PagePath.YouAreApprovedPerson)]
        public async Task<IActionResult> TeamMemberRoleInOrganisationAddAnother()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.YouAreApprovedPerson);
            DeleteFocusId();
            return await SaveSessionAndRedirect(session, nameof(TeamMemberRoleInOrganisation),
                PagePath.YouAreApprovedPerson, PagePath.TeamMemberRoleInOrganisation);
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
                IsLimitedPartnership = isPartnership && (session.ReExCompaniesHouseSession?.Partnership?.IsLimitedPartnership ?? false),
                IsApprovedUser = session.IsApprovedUser,
                ProducerType = session.ReExManualInputSession?.ProducerType,
                IsUkMainAddress = session.IsUkMainAddress
            };

            var id = GetFocusId();
            if (id.HasValue)
            {
                SetFocusId(id.Value);
            }

            return View(approvedPersonViewModel);
        }

        [HttpPost]
        [Route(PagePath.YouAreApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.YouAreApprovedPerson)]
        public async Task<IActionResult> YouAreApprovedPerson(bool inviteApprovedPerson, bool? isUkMainAddress = null)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.YouAreApprovedPerson);

            if (isUkMainAddress is false)
            {
                // Sole- trader non-UK 
                return await SaveSessionAndRedirectToPage(
                    session,
                    nameof(ManageControlOrganisation),
                    PagePath.AddAnApprovedPerson,
                    PagePath.ManageControlOrganisation);
            }

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
            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.SoleTraderContinue, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.MemberPartnership)]
        [OrganisationJourneyAccess(PagePath.MemberPartnership)]
        public async Task<IActionResult> MemberPartnership()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.MemberPartnership);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            YesNoAnswer? isMember = null;
            var id = GetFocusId();
            if (id.HasValue && session.ReExCompaniesHouseSession?.TeamMembers != null)
            {
                SetFocusId(id.Value);

                var index = session.ReExCompaniesHouseSession.TeamMembers.FindIndex(0, x => x.Id.Equals(id));
                isMember = index is >= 0 ? YesNoAnswer.Yes : YesNoAnswer.No;
            }

            var viewModel = new IsMemberPartnershipViewModel
            {
                IsMemberPartnership = isMember
            };

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

            var teamMemberId = GetFocusId() ?? Guid.NewGuid();

            var index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(teamMemberId));

            // Team memebr exists
            if (index is >= 0)
            {
                if (model.IsMemberPartnership == YesNoAnswer.No)
                {
                    session.ReExCompaniesHouseSession?.TeamMembers?.RemoveAll(x => x.Id == teamMemberId);
                }
                else
                {
                    session.ReExCompaniesHouseSession.TeamMembers[index.Value].Role = ReExTeamMemberRole.Member;
                }
            }
            else
            {
                if (model.IsMemberPartnership == YesNoAnswer.Yes)
                {
                    session.ReExCompaniesHouseSession.TeamMembers ??= new List<ReExCompanyTeamMember>();
                    session.ReExCompaniesHouseSession.TeamMembers.Add(new ReExCompanyTeamMember
                    {
                        Id = teamMemberId,
                        Role = ReExTeamMemberRole.Member
                    });
                }
            }

            SetFocusId(teamMemberId);
            return model.IsMemberPartnership == YesNoAnswer.Yes
                ? await SaveSessionAndRedirect(session, nameof(PartnerDetails), PagePath.MemberPartnership, PagePath.PartnerDetails)
                : await SaveSessionAndRedirect(session, "CanNotInviteThisPerson", PagePath.MemberPartnership, PagePath.CanNotInviteThisPerson);
        }

        [HttpGet]
        [Route(PagePath.MemberPartnershipAdd)]
        public async Task<IActionResult> MemberPartnershipAdd()
        {
            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(MemberPartnership),
                PagePath.YouAreApprovedPerson, PagePath.MemberPartnership);
        }

        [HttpGet]
        [Route(PagePath.MemberPartnershipEdit)]
        public async Task<IActionResult> MemberPartnershipEdit([FromQuery] Guid id)
        {
            SetFocusId(id);

            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(MemberPartnership),
                PagePath.CheckYourDetails, PagePath.MemberPartnership);
        }

        [HttpGet]
        [Route(PagePath.PartnerDetails)]
        [OrganisationJourneyAccess(PagePath.PartnerDetails)]
        public async Task<IActionResult> PartnerDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.PartnerDetails);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var viewModel = new PartnerDetailsViewModel();

            var id = GetFocusId();
            if (id.HasValue)
            {
                SetFocusId(id.Value);
                var index = session.ReExCompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index is >= 0)
                {
                    viewModel.Id = id;
                    viewModel.FirstName = session.ReExCompaniesHouseSession.TeamMembers[index.Value]?.FirstName;
                    viewModel.LastName = session.ReExCompaniesHouseSession.TeamMembers[index.Value]?.LastName;
                    viewModel.Telephone = session.ReExCompaniesHouseSession.TeamMembers[index.Value]?.TelephoneNumber;
                    viewModel.Email = session.ReExCompaniesHouseSession.TeamMembers[index.Value]?.Email;
                }

                SetFocusId(id.Value);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.PartnerDetails)]
        [OrganisationJourneyAccess(PagePath.PartnerDetails)]
        public async Task<IActionResult> PartnerDetails(PartnerDetailsViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var companiesHouseSession = session.ReExCompaniesHouseSession ?? new();
            var members = companiesHouseSession.TeamMembers ?? [];
            var index = members.FindIndex(0, x => x.Id.Equals(model?.Id));
            bool isExistingMember = index >= 0;
            var id = model?.Id ?? Guid.NewGuid();

            if (isExistingMember)
            {
                members[index].FirstName = model?.FirstName;
                members[index].LastName = model?.LastName;
                members[index].TelephoneNumber = model.Telephone;
                members[index].Email = model?.Email;
            }
            else
            {
                members.Add(new ReExCompanyTeamMember
                {
                    Id = id,
                    FirstName = model?.FirstName,
                    LastName = model?.LastName,
                    TelephoneNumber = model.Telephone,
                    Email = model?.Email,
                    Role = ReExTeamMemberRole.Member
                });
            }

            companiesHouseSession.TeamMembers = members;
            session.ReExCompaniesHouseSession = companiesHouseSession;
            SetFocusId(id);

            return await SaveSessionAndRedirect(
                session,
                nameof(TeamMembersCheckInvitationDetails),
                PagePath.PartnerDetails,
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
                IsManualInputFlow = !session.IsCompaniesHouseFlow,
                Nation = session.UkNation,
                IsNonUk = !(session.IsUkMainAddress ?? true),
                IsSoleTrader = session.ReExManualInputSession?.ProducerType == ProducerType.SoleTrader,
                TradingName = session.TradingName
            };

            if (viewModel.IsCompaniesHouseFlow)
            {
                var companyHouseSession = session.ReExCompaniesHouseSession;
                var company = companyHouseSession?.Company;
                viewModel.BusinessAddress = company?.BusinessAddress;
                viewModel.CompanyName = company?.Name;
                viewModel.CompaniesHouseNumber = company?.CompaniesHouseNumber;
                viewModel.RoleInOrganisation = companyHouseSession?.RoleInOrganisation;
                viewModel.IsOrganisationAPartnership = session.IsOrganisationAPartnership ?? false;
                viewModel.LimitedPartnershipPartners = companyHouseSession?.Partnership?.LimitedPartnership?.Partners;
                viewModel.IsLimitedLiabilityPartnership = companyHouseSession?.Partnership?.IsLimitedLiabilityPartnership ?? false;
                viewModel.reExCompanyTeamMembers = companyHouseSession?.TeamMembers;
            }

            if (viewModel.IsSoleTrader)
            {
                var manualInput = session.ReExManualInputSession;
                viewModel.ProducerType = manualInput?.ProducerType;
                viewModel.BusinessAddress = manualInput?.BusinessAddress;

                viewModel.reExCompanyTeamMembers = new List<ReExCompanyTeamMember>();
                var teamMember = manualInput?.TeamMembers?.FirstOrDefault();
                if (teamMember != null)
                {
                    viewModel.reExCompanyTeamMembers.Add(teamMember);
                }
            }

            if (viewModel.IsNonUk)
            {
                var manualInput = session.ReExManualInputSession;
                viewModel.ProducerType = manualInput?.ProducerType;
                viewModel.BusinessAddress = manualInput?.BusinessAddress;
                viewModel.TradingName = manualInput?.OrganisationName;
                viewModel.reExCompanyTeamMembers = manualInput?.TeamMembers;
                viewModel.Nation = manualInput?.UkRegulatorNation;
            }

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

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

            return View(new LimitedPartnershipPersonCanNotBeInvitedViewModel
            {
                Id = id,
                TheyManageOrControlOrganisation = session.TheyManageOrControlOrganisation,
                AreTheyIndividualInCharge = session.AreTheyIndividualInCharge
            });
        }

        [HttpPost]
        [Route(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
        [OrganisationJourneyAccess(PagePath.ApprovedPersonPartnershipCanNotBeInvited)]
        public async Task<IActionResult> PersonCanNotBeInvited(LimitedPartnershipPersonCanNotBeInvitedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.ApprovedPersonPartnershipCanNotBeInvited, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.CanNotInviteThisPerson)]
        [OrganisationJourneyAccess(PagePath.CanNotInviteThisPerson)]
        public async Task<IActionResult> CanNotInviteThisPerson([FromQuery] Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.CanNotInviteThisPerson);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(new LimitedPartnershipPersonCanNotBeInvitedViewModel { Id = id });
        }

        [HttpPost]
        [Route(PagePath.CanNotInviteThisPerson)]
        [OrganisationJourneyAccess(PagePath.CanNotInviteThisPerson)]
        public async Task<IActionResult> CanNotInviteThisPerson(LimitedPartnershipPersonCanNotBeInvitedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.CanNotInviteThisPerson, PagePath.CheckYourDetails);
        }

        [HttpGet]
        [Route(PagePath.CanNotInviteThisPersonAddEligible)]
        [OrganisationJourneyAccess(PagePath.CanNotInviteThisPerson)]
        public async Task<IActionResult> CanNotInviteThisPersonAddEligible()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            DeleteFocusId();
            return await SaveSessionAndRedirect(session, nameof(MemberPartnership), PagePath.CanNotInviteThisPerson, PagePath.MemberPartnership);
        }

        [HttpGet]
        [Route(PagePath.NonCompaniesHousePartnershipAddApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipAddApprovedPerson)]
        public async Task<IActionResult> NonCompaniesHousePartnershipAddApprovedPerson()

        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            bool isNonCompaniesHousePartnership = session.ReExManualInputSession?.ProducerType == ProducerType.Partnership;
            SetBackLink(session, PagePath.NonCompaniesHousePartnershipAddApprovedPerson);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            return View(new AddApprovedPersonViewModel { IsNonCompaniesHousePartnership = isNonCompaniesHousePartnership, InviteUserOption = session.InviteUserOption?.ToString() });
        }

        [HttpPost]
        [Route(PagePath.NonCompaniesHousePartnershipAddApprovedPerson)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipAddApprovedPerson)]
        public async Task<IActionResult> NonCompaniesHousePartnershipAddApprovedPerson(AddApprovedPersonViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                model.IsNonCompaniesHousePartnership = session.ReExManualInputSession?.ProducerType == ProducerType.Partnership;
                SetBackLink(session, PagePath.NonCompaniesHousePartnershipAddApprovedPerson);

                ModelState.ClearValidationState(nameof(model.InviteUserOption));
                ModelState.AddModelError(nameof(model.InviteUserOption), ApprovedPersonErrorMessage);

                return View(model);
            }

            session.InviteUserOption = model.InviteUserOption.ToEnumOrNull<InviteUserOptions>();

            if (model.InviteUserOption == nameof(InviteUserOptions.BeAnApprovedPerson))
            {
                session.IsApprovedUser = true;
                return await SaveSessionAndRedirect(session, nameof(YouAreApprovedPerson), PagePath.NonCompaniesHousePartnershipAddApprovedPerson, PagePath.YouAreApprovedPerson); // to do: new non companies house view
            }
            else if (model.InviteUserOption == nameof(InviteUserOptions.InviteAnotherPerson))
            {
                return await SaveSessionAndRedirect(session, nameof(NonCompaniesHousePartnershipTeamMemberRole), PagePath.NonCompaniesHousePartnershipAddApprovedPerson, PagePath.NonCompaniesHousePartnershipTheirRole);
            }
            else //(model.InviteUserOption == nameof(InviteUserOptions.InviteLater))
            {
                return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.NonCompaniesHousePartnershipAddApprovedPerson, PagePath.CheckYourDetails);
            }
        }

        [HttpGet]
        [Route(PagePath.NonCompaniesHousePartnershipTheirRole)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipTheirRole)]
        public async Task<IActionResult> NonCompaniesHousePartnershipTeamMemberRole()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            SetBackLink(session, PagePath.NonCompaniesHousePartnershipTheirRole);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var viewModel = new TeamMemberRoleInOrganisationViewModel();
            var id = GetFocusId();

            if (id.HasValue)
            {
                var index = session.ReExManualInputSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index is >= 0)
                {
                    viewModel.Id = id;
                    viewModel.RoleInOrganisation = session.ReExManualInputSession.TeamMembers[index.Value]?.Role;
                }
                SetFocusId(id.Value);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route(PagePath.NonCompaniesHousePartnershipTheirRole)]
        [OrganisationJourneyAccess(PagePath.NonCompaniesHousePartnershipTheirRole)]
        public async Task<IActionResult> NonCompaniesHousePartnershipTeamMemberRole(TeamMemberRoleInOrganisationViewModel model)
        {
            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.NonCompaniesHousePartnershipTheirRole);
                return View(model);
            }

            // Read existing approved persons, or create a fresh emtpy list if none present in session
            ReExManualInputSession nonCompaniesHouseSession = session!.ReExManualInputSession ?? new();
            List<ReExCompanyTeamMember> approvedPersons = nonCompaniesHouseSession.TeamMembers ?? [];
            int memberIndex = approvedPersons.FindIndex(0, x => x.Id.Equals(model?.Id));

            // ReExTeamMemberRole.None is an instruction to delete the approved person, should they exist
            if (model.RoleInOrganisation == ReExTeamMemberRole.None)
            {
                if (memberIndex < 0)
                {
                    // goes to "You cannot invite this person to be an approved person" page which is unavailable because its not been built
                    throw new NotImplementedException("You cannot invite this person to be an approved person");
                }

                approvedPersons.RemoveAt(memberIndex);
                nonCompaniesHouseSession.TeamMembers = approvedPersons;
                session.ReExManualInputSession = nonCompaniesHouseSession;

                // go to Check Your Details
                return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.CheckYourDetails),
                    PagePath.NonCompaniesHousePartnershipTheirRole, PagePath.CheckYourDetails);
            }

            // unable to find id, so adding a new approved person
            if (memberIndex < 0)
            {
                approvedPersons.Add(new ReExCompanyTeamMember { Id = Guid.NewGuid() });
                memberIndex = approvedPersons.Count - 1;
            }

            approvedPersons[memberIndex].Role = model.RoleInOrganisation;
            nonCompaniesHouseSession.TeamMembers = approvedPersons;
            session.ReExManualInputSession = nonCompaniesHouseSession;

            SetFocusId(approvedPersons[memberIndex].Id);

            // check the email, but any field other than Id will do to determine if its an existing approved person
            if (approvedPersons[memberIndex].Email?.Length > 0)
            {
                // goes to "Check invitation details" page which is unavailable because its not been built
                return await SaveSessionAndRedirect(session, nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberCheckInvitationDetails),
                    PagePath.NonCompaniesHousePartnershipTheirRole, PagePath.NonCompaniesHouseTeamMemberCheckInvitationDetails);
            }
            else
            {
                // goes to "What are their details?" page, but should use SetFocusId() rather than route values
                return await SaveSessionAndRedirect(session: session,
                    actionName: nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails),
                    currentPagePath: PagePath.NonCompaniesHousePartnershipTheirRole,
                    nextPagePath: PagePath.NonCompaniesHouseTeamMemberDetails,
                    controllerName: nameof(ApprovedPersonController),
                    routeValues: new { id = approvedPersons[memberIndex].Id });
            }
        }
    }
}
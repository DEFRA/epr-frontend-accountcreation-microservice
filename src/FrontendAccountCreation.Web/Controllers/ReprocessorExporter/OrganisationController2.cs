using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter
{
    [ExcludeFromCodeCoverage(Justification ="The pages before and after are not developed")]
    public partial class OrganisationController : Controller
    {
        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisation + "/{id:guid?}")]
        public async Task<IActionResult> TeamMemberRoleInOrganisation(Guid? id)
        {
            OrganisationSession session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();
            SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            // show previously selected team member role
            if (id.HasValue)
            {
                int? index = session.CompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index.GetValueOrDefault(-1) >= 0)
                {
                    TeamMemberRoleInOrganisationViewModel viewModel = new TeamMemberRoleInOrganisationViewModel
                    {
                        RoleInOrganisation = session.CompaniesHouseSession.TeamMembers[index.Value]?.Role
                    };
                    return View(viewModel);
                }
            }

            return View();
        }

        [HttpPost]
        [Route(PagePath.TeamMemberRoleInOrganisation + "/{id:guid?}")]
        public async Task<IActionResult> TeamMemberRoleInOrganisation(TeamMemberRoleInOrganisationViewModel model)
        {
            OrganisationSession? session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();
            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);
                return View(model);
            }

            ReExCompaniesHouseSession companiesHouseSession = session.CompaniesHouseSession ?? new();
            int? index = companiesHouseSession.TeamMembers?.FindIndex(0, x => x.Id.Equals(model?.Id));
            if (index.GetValueOrDefault(-1) >= 0)
            {
                // found existing team member
                session.CompaniesHouseSession.TeamMembers[index.Value].Role = model.RoleInOrganisation;
            }
            else
            {
                // add new team member
                List<ReExCompanyTeamMember> members = companiesHouseSession.TeamMembers ?? new();
                members.Add(new ReExCompanyTeamMember { Id = Guid.NewGuid(), Role = model.RoleInOrganisation });
                companiesHouseSession.TeamMembers = members;
                session.CompaniesHouseSession = companiesHouseSession;
            }

            // navigating to the wrong page, the correct page is not developed
            return await SaveSessionAndRedirect(session, nameof(TeamMembersDetails), PagePath.TeamMemberRoleInOrganisation,
                PagePath.TeamMemberDetails);

        }

        [HttpGet]
        [Route(PagePath.TeamMembersCheckInvitationDetails + "/{id:guid?}")]
        public async Task<IActionResult> TeamMembersCheckInvitationDetails(Guid? id)
        {
            OrganisationSession session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new();

            // if id is supplied, remove the team member
            if (id.HasValue)
            {
                int? index = session.CompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));
                if (index.GetValueOrDefault(-1) >= 0)
                {
                    session.CompaniesHouseSession.TeamMembers.RemoveAt(index.Value);
                }
            }

            SetBackLink(session, PagePath.TeamMembersCheckInvitationDetails);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            return View(session.CompaniesHouseSession?.TeamMembers);
        }

        [HttpGet]
        [Route(PagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMembersDetails()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();

            SetBackLink(session, PagePath.TeamMemberDetails);
            ViewBag.BackLinkToDisplay = PagePath.TeamMemberRoleInOrganisation;

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var teamMember = GetCurrentTeamMember(session);

            if (teamMember != null)
            {
                var viewModel = new TeamMemberViewModel
                {
                    FullName = teamMember.FullName,
                    Telephone = teamMember.TelephoneNumber,
                    Email = teamMember.Email
                };

                return View(viewModel);
            }

            return View();
        }


        [HttpPost]
        [Route(PagePath.TeamMemberDetails)]
        public async Task<IActionResult> TeamMembersDetails(TeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                ViewBag.BackLinkToDisplay = PagePath.TeamMemberRoleInOrganisation;
                return View(model);
            }

            var index = session.CompaniesHouseSession?.CurrentTeamMemberIndex;

            if (index.HasValue && session.CompaniesHouseSession?.TeamMembers != null && index.Value < session.CompaniesHouseSession.TeamMembers.Count)
            {
                var teamMember = session.CompaniesHouseSession.TeamMembers[index.Value];

                teamMember.FullName = model.FullName;
                teamMember.TelephoneNumber = model.Telephone;
                teamMember.Email = model.Email;
            }

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);


            return RedirectToAction("");
        }


        private ReExCompanyTeamMember? GetCurrentTeamMember(OrganisationSession session)
        {
            var members = session.CompaniesHouseSession?.TeamMembers;
            var index = session.CompaniesHouseSession?.CurrentTeamMemberIndex;

            return (index.HasValue && members != null && index.Value < members.Count)
                ? members[index.Value]
                : null;
        }

    }
}

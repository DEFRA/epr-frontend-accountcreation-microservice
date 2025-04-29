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
            Guid queryStringId;
            bool isExistingMember = false;

            if (index.GetValueOrDefault(-1) >= 0)
            {
                queryStringId = model.Id.Value;
                isExistingMember = true;

                // found existing team member, set their role
                session.CompaniesHouseSession.TeamMembers[index.Value].Role = model.RoleInOrganisation;

            }
            else
            {
                queryStringId = Guid.NewGuid();

                // add new team member
                List<ReExCompanyTeamMember> members = companiesHouseSession.TeamMembers ?? new();
                members.Add(new ReExCompanyTeamMember { Id = queryStringId, Role = model.RoleInOrganisation });
                companiesHouseSession.TeamMembers = members;
                session.CompaniesHouseSession = companiesHouseSession;

            }

            session.IsUserChangingDetails = false;
            await SaveSession(session, PagePath.TeamMemberRoleInOrganisation, PagePath.TeamMemberDetails + $"/{queryStringId}");

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

        /// <summary>
        /// Show team member details enetered so far
        /// </summary>
        /// <param name="id">Id of team member to remove</param>
        /// <returns></returns>
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

            return View(session.CompaniesHouseSession?.TeamMembers?.Where(x => !string.IsNullOrWhiteSpace(x.FullName)).ToList());
            //return View(session.CompaniesHouseSession?.TeamMembers);

        }

        [HttpGet]
        [Route(PagePath.TeamMemberDetails + "/{id:guid}")]
        public async Task<IActionResult> TeamMemberDetails(Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();

            SetBackLink(session, PagePath.TeamMemberDetails);
            //ViewBag.BackLinkToDisplay = PagePath.TeamMemberRoleInOrganisation;

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            int? index = session.CompaniesHouseSession?.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));

            if (index.GetValueOrDefault(-1) >= 0)
            {
                var viewModel = new TeamMemberViewModel
                {
                    Id = id,
                    FullName = session.CompaniesHouseSession.TeamMembers[index.Value].FullName,
                    Telephone = session.CompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber,
                    Email = session.CompaniesHouseSession.TeamMembers[index.Value].Email
                };

                return View(viewModel);
            }

            return View();
        }


        [HttpPost]
        [Route(PagePath.TeamMemberDetails + "/{id:guid?}")]
        public async Task<IActionResult> TeamMemberDetails(Guid? id, TeamMemberViewModel model)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                ViewBag.BackLinkToDisplay = PagePath.TeamMemberDetails;
                return View(model);
            }

            ReExCompaniesHouseSession companiesHouseSession = session.CompaniesHouseSession ?? new();
            int? index = companiesHouseSession.TeamMembers?.FindIndex(0, x => x.Id.Equals(id));

            if (index.GetValueOrDefault(-1) >= 0)
            {
                // found existing team member
                session.CompaniesHouseSession.TeamMembers[index.Value].FullName = model.FullName;
                session.CompaniesHouseSession.TeamMembers[index.Value].TelephoneNumber = model.Telephone;
                session.CompaniesHouseSession.TeamMembers[index.Value].Email = model.Email;
            }

            // go to check invitation details summary page for all team members
            return await SaveSessionAndRedirect(session, nameof(TeamMembersCheckInvitationDetails), PagePath.TeamMemberDetails,
                PagePath.TeamMembersCheckInvitationDetails);
        }

    }
}

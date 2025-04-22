using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter
{
    [ExcludeFromCodeCoverage(Justification ="The pages before and after are not developed")]
    public partial class OrganisationController : Controller
    {
        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new OrganisationSession();
            SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            int? teamMemberIndex = session.CompaniesHouseSession?.CurrentTeamMemberIndex;
            if (teamMemberIndex.HasValue)
            {
                var members = session.CompaniesHouseSession?.TeamMembers;
                if (members?.Count > teamMemberIndex.Value)
                {
                    var viewModel = new TeamMemberRoleInOrganisationViewModel
                    {
                        RoleInOrganisation = members[teamMemberIndex.Value]?.Role
                    };
                    return View(viewModel);
                }
            }

            return View();
        }

        [HttpPost]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        [AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation(TeamMemberRoleInOrganisationViewModel model, string? invite)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            if (!ModelState.IsValid)
            {
                SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);
                return View(model);
            }

            FillSessionWithALoadOfNonsense(model.RoleInOrganisation.Value);
            bool isInvited = invite != "without-invite";
            session.CompaniesHouseSession.TeamMembers[session.CompaniesHouseSession.CurrentTeamMemberIndex].IsInvited = isInvited;

            if (isInvited)
            {
                // navigating to the wrong page, the correct page is not developed
                return await SaveSessionAndRedirect(session, nameof(ConfirmDetailsOfTheCompany), PagePath.TeamMemberRoleInOrganisation,
                    PagePath.ConfirmCompanyDetails);

            }

            // navigating to the wrong page, the correct page is not developed
            return await SaveSessionAndRedirect(session, nameof(CompaniesHouseNumber), PagePath.TeamMemberRoleInOrganisation,
                PagePath.CompaniesHouseNumber);

            // for development only because the next page is not ready
            void FillSessionWithALoadOfNonsense(ReExTeamMemberRole role)
            {
                Contact contact = new Contact();
                contact.FirstName = "Bloke";
                contact.LastName = "Downpub";
                contact.TelephoneNumber = "06783327395";
                session.Contact = contact;
                session.UkNation = Nation.Scotland;

                ReExCompaniesHouseSession companiesHouseSession = new ReExCompaniesHouseSession();
                var address = new Address();
                address.Town = "Chesterfield";
                var company = new Company();
                company.Name = "Snibbo Widgets Ltd";
                company.CompaniesHouseNumber = role.ToString();
                company.BusinessAddress = address;

                companiesHouseSession.Company = company;
                session.OrganisationType = OrganisationType.CompaniesHouseCompany;
                companiesHouseSession.RoleInOrganisation = (RoleInOrganisation) role;

                int? teamMemberIndex = session.CompaniesHouseSession?.CurrentTeamMemberIndex;
                if (teamMemberIndex.HasValue)
                {
                    var members = session.CompaniesHouseSession?.TeamMembers;
                    if (members?.Count > teamMemberIndex.Value)
                    {
                        members[teamMemberIndex.Value].Role = role;
                        companiesHouseSession.TeamMembers = members;
                        session.CompaniesHouseSession = companiesHouseSession;
                        return;
                    }
                }

                var newMembers = new List<ReExCompanyTeamMember>();
                ReExCompanyTeamMember newMember = new ReExCompanyTeamMember
                {
                    Role = role
                };
                newMembers.Add(newMember);

                companiesHouseSession.TeamMembers = newMembers;
                companiesHouseSession.CurrentTeamMemberIndex = 0;
                session.CompaniesHouseSession = companiesHouseSession;

            }
        }
    }
}

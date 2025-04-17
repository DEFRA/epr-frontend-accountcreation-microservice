using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Controllers.AccountCreation
{
    [ExcludeFromCodeCoverage(Justification ="The pages before and after are not developed")]
    public partial class AccountCreationController : Controller
    {
        [HttpGet]
        [Route(PagePath.TeamMemberRoleInOrganisation)]
        public async Task<IActionResult> TeamMemberRoleInOrganisation()
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new AccountCreationSession();
            SetBackLink(session, PagePath.TeamMemberRoleInOrganisation);
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

            var viewModel = new TeamMemberRoleInOrganisationViewModel()
            {
                RoleInOrganisation = session.CompaniesHouseSession?.TeamMemberRoleInOrganisation
            };

            return View(viewModel);
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

            if (invite != null && invite == "without-invite")
            {
                FillSessionWithALoadOfNonsense(model.RoleInOrganisation.Value);

                return await SaveSessionAndRedirect(session, nameof(CheckYourDetails), PagePath.TeamMemberRoleInOrganisation,
                    PagePath.CheckYourDetails);
            }

            // navigating to the wrong page, the correct page is not developed
            return await SaveSessionAndRedirect(session, nameof(Declaration), PagePath.TeamMemberRoleInOrganisation,
                    PagePath.Declaration);

            // for development only because the next page is not ready
            void FillSessionWithALoadOfNonsense(Core.Sessions.TeamMemberRoleInOrganisation role)
            {
                Contact contact = new Contact();
                contact.FirstName = "Bloke";
                contact.LastName = "Downpub";
                contact.TelephoneNumber = "06783327395";
                session.Contact = contact;
                session.UkNation = Nation.Scotland;

                var companiesHouseSession = new CompaniesHouseSession();
                var address = new Address();
                address.Town = "Chesterfield";
                var company = new Company();
                company.Name = "Snibbo Widgets Ltd";
                company.CompaniesHouseNumber = "4567890";
                company.BusinessAddress = address;

                companiesHouseSession.Company = company;
                session.OrganisationType = OrganisationType.CompaniesHouseCompany;
                companiesHouseSession.RoleInOrganisation = (Core.Sessions.RoleInOrganisation) role;
                companiesHouseSession.TeamMemberRoleInOrganisation = role;
                session.CompaniesHouseSession = companiesHouseSession;
            }
        }
    }
}

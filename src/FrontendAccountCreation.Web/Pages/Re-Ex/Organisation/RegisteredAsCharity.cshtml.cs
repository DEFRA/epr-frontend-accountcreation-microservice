using System.ComponentModel.DataAnnotations;
using System.Net;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.Radios;
using FrontendAccountCreation.Web.FullPages.Radios.Common;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;

[Feature(FeatureFlags.AddOrganisationCompanyHouseDirectorJourney)]
[AuthorizeForScopes(ScopeKeySection = ConfigKeys.FacadeScope)]
public class RegisteredAsCharity(
    ISessionManager<OrganisationSession> sessionManager,
    IStringLocalizer<SharedResources> sharedLocalizer,
    IStringLocalizer<RegisteredAsCharity> localizer)
    : OrganisationPageModel<RegisteredAsCharity>(sessionManager, sharedLocalizer, localizer), IRadiosPageModel
{
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo(SharedLocalizer);

    [BindProperty]
    [Required(ErrorMessage = "RegisteredAsCharity.ErrorMessage")]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance;

    public string? Legend => Localizer["RegisteredAsCharity.Question"];

    public string? Hint => Localizer["RegisteredAsCharity.Description"];
    
    public async Task<IActionResult> OnGet(
        [FromServices] IOptions<DeploymentRoleOptions> deploymentRoleOptions)
    {
        if (deploymentRoleOptions.Value.IsRegulator())
        {
            return RedirectToAction(nameof(ErrorController.ErrorReEx), nameof(ErrorController).Replace("Controller", ""), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        SelectedValue = session?.IsTheOrganisationCharity?.ToString();

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Errors = ErrorStateFromModelState.Create(ModelState);
            return Page();
        }

        var session = await SessionManager.GetSessionAsync(HttpContext.Session)
            ?? new OrganisationSession
            {
                Journey = [PagePath.RegisteredAsCharity]
            };

        session.IsTheOrganisationCharity = bool.Parse(SelectedValue!);

        if (session.IsTheOrganisationCharity == true)
        {
            return await SaveSessionAndRedirect(session, nameof(OrganisationController),
                nameof(OrganisationController.NotAffected), PagePath.RegisteredAsCharity, PagePath.NotAffected);
        }
        return await SaveSessionAndRedirect(session, nameof(OrganisationController),
            nameof(OrganisationController.RegisteredWithCompaniesHouse), PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse);
    }
}
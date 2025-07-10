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
    IStringLocalizer<RegisteredAsCharity> localizer, 
    IOptions<ExternalUrlsOptions> externalUrlsOptions)
    : OrganisationPageModel<RegisteredAsCharity>(PagePath.RegisteredAsCharity, sessionManager, sharedLocalizer, localizer),
        IRadiosPageModel
{
    public IEnumerable<IRadio> Radios => CommonRadios.YesNo_IsHeSheIt(SharedLocalizer);

    [BindProperty]
    [Required(ErrorMessage = "RegisteredAsCharity.ErrorMessage")]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance;
    
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

        var session = await SetupRegisteredAsCharityPage();

        SelectedValue = session?.IsTheOrganisationCharity?.ToString();

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var session = await SetupRegisteredAsCharityPage()
                      ?? new OrganisationSession
                      {
                          Journey = [PagePath.RegisteredAsCharity]
                      };

        if (!ModelState.IsValid)
        {
            Errors = ErrorStateFromModelState.Create(ModelState);
            return Page();
        }

        session.IsTheOrganisationCharity = bool.Parse(SelectedValue!);

        if (session.IsTheOrganisationCharity == true)
        {
            return await SaveSessionAndRedirect(session, nameof(OrganisationController),
                nameof(OrganisationController.NotAffected), PagePath.NotAffected);
        }
        return await SaveSessionAndRedirect(session, nameof(OrganisationController),
            nameof(OrganisationController.RegisteredWithCompaniesHouse), PagePath.RegisteredWithCompaniesHouse);
    }

    private async Task<OrganisationSession?> SetupRegisteredAsCharityPage()
    {
        ViewData["Title"] = Title;
        ViewData["ApplicationTitleOverride"] = LayoutOverrides.ReExTitleOverride;
        ViewData["HeaderOverride"] = LayoutOverrides.ReExOrganisationHeaderOverride;

        ViewData["BackLinkToDisplay"] = externalUrlsOptions.Value.PrnRedirectUrl;

        return await SessionManager.GetSessionAsync(HttpContext.Session);
    }
}
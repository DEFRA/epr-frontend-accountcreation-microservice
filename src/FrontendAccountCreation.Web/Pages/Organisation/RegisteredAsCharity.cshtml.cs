using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.Radios;
using FrontendAccountCreation.Web.FullPages.Radios.Common;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace FrontendAccountCreation.Web.Pages.Organisation;

public class RegisteredAsCharity : OrganisationPageModel, IRadiosPageModel
{
    private readonly IStringLocalizer<RegisteredAsCharity> _localizer;

    public RegisteredAsCharity(
        ISessionManager<OrganisationSession> sessionManager,
        IStringLocalizer<SharedResources> sharedLocalizer,
        IStringLocalizer<RegisteredAsCharity> localizer)
        : base(sessionManager, sharedLocalizer)
    {
        _localizer = localizer;
    }

    public IEnumerable<IRadio> Radios => CommonRadios.YesNo(SharedLocalizer);

    [BindProperty]
    [Required(ErrorMessage = "RegisteredAsCharity.ErrorMessage")]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorStateEmpty.Instance; //ErrorState.Empty;

    public string? Legend => _localizer["RegisteredAsCharity.Question"];

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

        if (session?.IsTheOrganisationCharity != null)
        {
            SelectedValue = session.IsTheOrganisationCharity.ToString();
        }

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
            ?? new OrganisationSession()
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
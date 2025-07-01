using FrontendAccountCreation.Core.Extensions;
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
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Html;

namespace FrontendAccountCreation.Web.Pages.Organisation;

public class OrganisationPageModel(
    ISessionManager<OrganisationSession> sessionManager,
    IStringLocalizer<SharedResources> sharedLocalizer)
    : PageModel
{
    protected ISessionManager<OrganisationSession> SessionManager { get; } = sessionManager;
    protected IStringLocalizer<SharedResources> SharedLocalizer { get; } = sharedLocalizer;

    protected async Task<RedirectToActionResult> SaveSessionAndRedirect(
        OrganisationSession session,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    private async Task SaveSession(
        OrganisationSession session,
        string currentPagePath,
        string? nextPagePath)
    {
        var index = session.Journey.FindIndex(x => x != null && x.Contains(currentPagePath.Split("?")[0]));

        // this also cover if current page not found (index = -1) then it clears all pages
        session.Journey = session.Journey.Take(index + 1).ToList();

        session.Journey.AddIfNotExists(nextPagePath);

        AddPageToWhiteList(session, currentPagePath);
        AddPageToWhiteList(session, nextPagePath);

        await SessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private void AddPageToWhiteList(
        OrganisationSession session,
        string currentPagePath)
    {
        if (!string.IsNullOrEmpty(currentPagePath))
        {
            session.WhiteList.Add(currentPagePath);
        }
    }
}

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

    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

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
            return await SaveSessionAndRedirect(session, nameof(OrganisationController.NotAffected), PagePath.RegisteredAsCharity, PagePath.NotAffected);
        }
        return await SaveSessionAndRedirect(session, nameof(OrganisationController.RegisteredWithCompaniesHouse), PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse);
    }
}
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.ErrorNext;
using FrontendAccountCreation.Web.FullPages.Radios;
using FrontendAccountCreation.Web.FullPages.Radios.Common;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Net;
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

namespace FrontendAccountCreation.Web.Pages.Organisation;

public class OrganisationPageModel(ISessionManager<OrganisationSession> sessionManager) : PageModel
{
    protected ISessionManager<OrganisationSession> SessionManager = sessionManager;

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

        await sessionManager.SaveSessionAsync(HttpContext.Session, session);
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

public class RegisteredAsCharityModel : OrganisationPageModel, IRadiosPageModel
{
    public RegisteredAsCharityModel(ISessionManager<OrganisationSession> sessionManager)
        : base(sessionManager)
    {
    }

    public IEnumerable<IRadio> Radios => CommonRadios.YesNo;

    [BindProperty]
    public string? SelectedValue { get; set; }

    public IErrorState Errors { get; set; } = ErrorState.Empty;

    public string? Legend => "RegisteredAsCharity.Question";

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
        //todo: add new IError to work with model error state

        //if (SelectedValue == null)
        //{
        //    Errors = ErrorState.Create(PossibleErrors, ErrorId.NoCountrySelected);
        //    return;
        //}

        //SelectedCountry = (Country)Enum.Parse(typeof(Country), SelectedValue);

        if (!ModelState.IsValid)
        {
            //ModelState
            Errors = ErrorStateFromModelState.Create(ModelState);
            return Page();
        }

        var session = await SessionManager.GetSessionAsync(HttpContext.Session)
            ?? new OrganisationSession()
            {
                Journey = [PagePath.RegisteredAsCharity]
            };

        //session.IsTheOrganisationCharity = model.isTheOrganisationCharity == YesNoAnswer.Yes;

        //if (session.IsTheOrganisationCharity.Value)
        //{
        //    return await SaveSessionAndRedirect(session, nameof(NotAffected), PagePath.RegisteredAsCharity, PagePath.NotAffected);
        //}
        //else
        //{
        //    return await SaveSessionAndRedirect(session, nameof(RegisteredWithCompaniesHouse), PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse);
        //}

        session.IsTheOrganisationCharity = bool.Parse(SelectedValue!);

        if (session.IsTheOrganisationCharity == true)
        {
            return await SaveSessionAndRedirect(session, nameof(OrganisationController.NotAffected), PagePath.RegisteredAsCharity, PagePath.NotAffected);
        }
        return await SaveSessionAndRedirect(session, nameof(OrganisationController.RegisteredWithCompaniesHouse), PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse);
    }

    public enum ErrorId
    {
        NoCountrySelected
    }

    public static readonly ImmutableDictionary<int, PossibleError> PossibleErrors =
        ImmutableDictionary.Create<int, PossibleError>()
            .Add(ErrorId.NoCountrySelected, "Select the country where you live");
}
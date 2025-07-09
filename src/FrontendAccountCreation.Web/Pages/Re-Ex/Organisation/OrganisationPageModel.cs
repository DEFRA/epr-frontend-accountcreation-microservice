using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Extensions;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace FrontendAccountCreation.Web.Pages.Re_Ex.Organisation;

// could have classes between this and the derived classes,
// so that the most derived classes only have to declare the enum as a type parameter
// an expression to get the value to/from the session
// and the actions for each enum value
public class OrganisationPageModel<T>(
    string pagePath,
    ISessionManager<OrganisationSession> sessionManager,
    IStringLocalizer<SharedResources> sharedLocalizer,
    IStringLocalizer<T> localizer)
    : PageModel
    where T : OrganisationPageModel<T>
{
    public string Path { get; } = pagePath;
    protected ISessionManager<OrganisationSession> SessionManager { get; } = sessionManager;
    protected IStringLocalizer<SharedResources> SharedLocalizer { get; } = sharedLocalizer;
    protected IStringLocalizer<T> Localizer { get; } = localizer;

    private string? _question;
    private string? _title;
    private string? _hint;

    public virtual string Title
    {
        get => _title ?? Localizer[$"{typeof(T).Name}.Title"];
        protected set => _title = value;
    }

    public virtual string Question
    {
        get => _question ?? Localizer[$"{typeof(T).Name}.Question"];
        protected set => _question = value;
    }

    public virtual string? Hint
    {
        get
        {
            if (_hint != null)
            {
                return _hint;
            }
            var localized = Localizer[$"{typeof(T).Name}.Hint"];
            return localized.ResourceNotFound ? null : localized.Value;
        }
        protected set => _hint = value;
    }

    public virtual string ButtonText => SharedLocalizer["Continue"];

    protected async Task<OrganisationSession> SetupPage()
    {
        ViewData["Title"] = Title;
        ViewData["ApplicationTitleOverride"] = LayoutOverrides.ReExTitleOverride;
        ViewData["HeaderOverride"] = LayoutOverrides.ReExOrganisationHeaderOverride;

        var session = await SessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session!);

        return session!;
    }

    public void SetBackLink(OrganisationSession session)
    {
        if (session.IsUserChangingDetails && Path != PagePath.CheckYourDetails)
        {
            ViewData["BackLinkToDisplay"] = PagePath.CheckYourDetails;
        }
        else
        {
            ViewData["BackLinkToDisplay"] = session.Journey.PreviousOrDefault(Path) ?? string.Empty;
        }
    }

    //to-do: change these so don't have to pass actionName and nextPagePath
    // (we should be able to get one from the other and remove a pit of failure)
    protected async Task<RedirectToActionResult> SaveSessionAndRedirect(
        OrganisationSession session,
        string controllerName,
        string actionName,
        string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, Path, nextPagePath);

        return RedirectToAction(actionName, controllerName.WithoutControllerSuffix());
    }

    private async Task SaveSession(
        OrganisationSession session,
        string currentPagePath,
        string? nextPagePath)
    {
        var index = session.Journey.FindIndex(x => x == currentPagePath.Split("?")[0]);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.Journey = session.Journey.Take(index + 1).ToList();

        session.Journey.AddIfNotExists(nextPagePath);

        AddPageToWhiteList(session, currentPagePath);
        AddPageToWhiteList(session, nextPagePath);

        await SessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private static void AddPageToWhiteList(
        OrganisationSession session,
        string currentPagePath)
    {
        if (!string.IsNullOrEmpty(currentPagePath))
        {
            session.WhiteList.Add(currentPagePath);
        }
    }
}
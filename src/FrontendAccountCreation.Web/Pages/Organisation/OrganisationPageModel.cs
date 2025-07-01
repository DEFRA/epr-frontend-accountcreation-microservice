using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Extensions;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

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

    protected async Task<RedirectToActionResult> SaveSessionAndRedirect(
        OrganisationSession session,
        string controllerName,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName, controllerName.WithoutControllerSuffix());
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
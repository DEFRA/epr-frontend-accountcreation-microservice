using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions.Interfaces;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.Controllers;
using FrontendAccountCreation.Web.Extensions;

namespace FrontendAccountCreation.Web.Controllers;

public abstract class ControllerBase<T> : Controller where T : ILocalSession, new()
{
    private readonly ISessionManager<T> _sessionManager;

    protected ControllerBase(ISessionManager<T> sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public void SetBackLink(T session, string currentPagePath)
    {
        if (session.IsUserChangingDetails && currentPagePath != PagePath.CheckYourDetails)
        {
            ViewBag.BackLinkToDisplay = PagePath.CheckYourDetails;
        }
        else
        {
            ViewBag.BackLinkToDisplay = session.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
        }
    }

    public async Task<RedirectToActionResult> SaveSessionAndRedirect(T session,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    public async Task<RedirectToActionResult> SaveSessionAndRedirect(T session,
        string controllerName,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName, controllerName.WithoutControllerSuffix());
    }

    public async Task<RedirectToActionResult> SaveSessionAndRedirect(T session,
        string actionName,
        string currentPagePath,
        string? nextPagePath,
        string? controllerName,
        object? routeValues)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        if (!string.IsNullOrWhiteSpace(controllerName))
        {
            return RedirectToAction(actionName, controllerName.WithoutControllerSuffix(), routeValues);
        }
        else
        {
            return RedirectToAction(actionName, routeValues);
        }
    }

    public async Task SaveSession(T session, string currentPagePath, string? nextPagePath)
    {
        var index = session.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.Journey = session.Journey.Take(index + 1).ToList();

        session.Journey.AddIfNotExists(nextPagePath);

        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }
}
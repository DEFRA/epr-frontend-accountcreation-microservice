using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.Interfaces;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountCreation.Web.Sessions;

namespace FrontendAccountCreation.Web.Controllers.ReprocessorExporter;

public abstract class ControllerBase<T> : Controller where T : ILocalSession, new()
{
    private readonly ISessionManager<T> _sessionManager;

    public ControllerBase(ISessionManager<T> sessionManager)
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

    public async Task<RedirectToActionResult> SaveSessionAndRedirect(
        T session,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    public async Task<RedirectToActionResult> SaveSessionAndRedirect(
        T session,
        string controllerName,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        session.IsUserChangingDetails = false;
        var contNameWOCont = controllerName.Replace("Controller", string.Empty);
        await SaveSession(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName, contNameWOCont);
    }

    public async Task<RedirectToActionResult> SaveSessionAndRedirect(T session,
        string actionName,
        string currentPagePath,
        string? nextPagePath,
        string? controllerName = null,
        object? routeValues = null)
    {
        session.IsUserChangingDetails = false;
        await SaveSession(session, currentPagePath, nextPagePath);

        return !string.IsNullOrWhiteSpace(controllerName) ? RedirectToAction(actionName, controllerName, routeValues) : RedirectToAction(actionName, routeValues);
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
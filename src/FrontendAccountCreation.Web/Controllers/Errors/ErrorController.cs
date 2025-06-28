using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using FrontendAccountCreation.Core.Utilities;

namespace FrontendAccountCreation.Web.Controllers.Errors;

[AllowAnonymous]
public class ErrorController(AllowList<string> reExControllerNames) : Controller
{
    [Route(PagePath.Error)]
    public ViewResult Error(int? statusCode)
    {
        string generalErrorPage = ViewNames.Error;
        string pageNotFoundPage = ViewNames.PageNotFound;

        // we could either look at whether exceptionhandler is present or by the passed status code
        // but this way works irrespective of the passed status code
        var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandler != null)
        {
            // this is an unhandled exception, not a 404

            // alternatively, we could look for /re-ex/ in exceptionHandler.Path
            string? controllerName = (string?)exceptionHandler.RouteValues?["Controller"];
            if (controllerName != null && reExControllerNames.IsAllowed(controllerName))
            {
                generalErrorPage = ViewNames.ErrorReEx;
            }
            Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Always 500 for exceptions
            return View(generalErrorPage, new ErrorViewModel());
        }
        
        // This is a status code error (like 404)
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        if (statusCodeReExecuteFeature?.OriginalPath.Contains("re-ex", StringComparison.OrdinalIgnoreCase) == true)
        {
            pageNotFoundPage = ViewNames.PageNotFoundReEx;
        }
        Response.StatusCode = statusCode ?? (int)HttpStatusCode.InternalServerError;
        var errorView = statusCode == (int?)HttpStatusCode.NotFound ? pageNotFoundPage : generalErrorPage;
        return View(errorView, new ErrorViewModel());
    }

    /// <summary>
    /// Use if you want to redirect the user directly to the ReEx error page.
    /// </summary>
    /// <param name="statusCode">An optional status code to return with the page. Defaults to 200.</param>
    [Route(PagePath.ErrorReEx)]
    public ViewResult ErrorReEx(int? statusCode)
    {
        Response.StatusCode = statusCode ?? (int)HttpStatusCode.InternalServerError;

        return View(new ErrorViewModel());
    }

    [Route(PagePath.PageNotFound)]
    public ViewResult PageNotFound()
    {
        Response.StatusCode = (int)HttpStatusCode.NotFound;
        return View(ViewNames.PageNotFound, new ErrorViewModel());
    }

    /// <summary>
    /// Use if you want to redirect the user directly to the ReEx "page not found" page.
    /// </summary>
    [Route(PagePath.PageNotFoundReEx)]
    public ViewResult PageNotFoundReEx()
    {
        Response.StatusCode = (int)HttpStatusCode.NotFound;

        return View(new ErrorViewModel());
    }
}

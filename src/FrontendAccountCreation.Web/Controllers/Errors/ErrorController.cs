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
        var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        string pageNotFoundPage = PagePath.PageNotFound;
        string generalErrorPage = PagePath.Error;

        if (exceptionHandler != null)
        {
            // exceptionHandler is null for 404's

            // alternatively, we could look for /re-ex/ in exceptionHandler.Path
            string? controllerName = (string?)exceptionHandler.RouteValues?["Controller"];

            if (controllerName != null && reExControllerNames.IsAllowed(controllerName))
            {
                generalErrorPage = PagePath.ErrorReEx;
            }

            Response.StatusCode = 200;
        }
        else
        {
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature?.OriginalPath.Contains("re-ex", StringComparison.OrdinalIgnoreCase) == true)
            {
                pageNotFoundPage = PagePath.PageNotFoundReEx;
            }

            Response.StatusCode = 404;
        }

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
        //todo: return 200 always?
        Response.StatusCode = statusCode ?? 200;

        return View(new ErrorViewModel());
    }
}

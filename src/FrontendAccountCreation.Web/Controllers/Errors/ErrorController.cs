using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using System.Net;
using FrontendAccountCreation.Core.Utilities;

namespace FrontendAccountCreation.Web.Controllers.Errors;

[AllowAnonymous]
public class ErrorController(AllowList<string> reExControllerNames) : Controller
{
    //private readonly AllowList<string> _reExControllerNames = reExControllerNames;

    [Route(PagePath.Error)]
    public ViewResult Error(int? statusCode)
    {
        var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();//<IExceptionHandlerFeature>();

        string generalErrorPage = "Error";

        if (exceptionHandler != null)
        {
            // exceptionHandler is null for 404's

            // alternatively, we could look for /re-ex/ in exceptionHandler.Path
            string? controllerName = (string?)exceptionHandler.RouteValues?["Controller"];

            if (controllerName != null && reExControllerNames.IsAllowed(controllerName))
            {
                generalErrorPage = "ErrorReEx";
            }
        }

        var errorView = statusCode == (int?)HttpStatusCode.NotFound ? PagePath.PageNotFound : generalErrorPage;

        Response.StatusCode = 200;

        return View(errorView, new ErrorViewModel());
    }

    [Route("error-inject")]
    public ViewResult ErrorInject()
    {
        throw new NotImplementedException();
    }
}

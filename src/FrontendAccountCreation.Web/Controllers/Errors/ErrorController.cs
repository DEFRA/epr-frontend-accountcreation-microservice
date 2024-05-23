using System.Net;

using FrontendAccountCreation.Web.Constants;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.Errors;

[AllowAnonymous]
public class ErrorController : Controller
{
    
    [Route(PagePath.Error)]
    public ViewResult Error(int? statusCode)
    {
        var errorView = statusCode == (int?)HttpStatusCode.NotFound ? PagePath.PageNotFound : "Error";

        Response.StatusCode = statusCode ?? (int)HttpStatusCode.InternalServerError;

        return View(errorView);
    }
}

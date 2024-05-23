using System.Net;

using FrontendAccountCreation.Web.Constants;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.Errors;

[AllowAnonymous]
public class AzureErrorController : Controller
{
    
    [Route(PagePath.AError)]
    public ViewResult Error(int? statusCode)
    {
        var errorView = statusCode == (int?)HttpStatusCode.NotFound ? PagePath.PageNotFound  : "AError";

        Response.StatusCode = statusCode ?? (int)HttpStatusCode.InternalServerError;

        return View(errorView);
    }
}

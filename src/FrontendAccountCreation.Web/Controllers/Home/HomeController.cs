using FrontendAccountCreation.Web.Constants;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.Home;

public class HomeController : Controller
{
    [AllowAnonymous]
    [Route(PagePath.SignedOut)]
    public IActionResult SignedOut(string? applicationTitleOverride, string? headerOverride)
    {
        HttpContext.Session.Clear();
        return View();
    }

    [AllowAnonymous]
    public IActionResult SignedOutInvalidToken()
    {
        HttpContext.Session.Clear();
        return View("InvalidToken");
    }

    [HttpGet]
    [Route(PagePath.UserAlreadyExists)]
    public IActionResult UserAlreadyExists()
    {
        return View();
    }
}

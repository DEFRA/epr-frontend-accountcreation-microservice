using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.Home;

public class HomeController : Controller
{
    [AllowAnonymous]
    [Route(PagePath.SignedOut)]
    public IActionResult SignedOut(bool? reEx)
    {
        HttpContext.Session.Clear();
        return View(reEx == true ? "SignedOutReEx" : "SignedOut");
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

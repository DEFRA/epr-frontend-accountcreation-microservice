﻿using FrontendAccountCreation.Web.Constants;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.Home;

public class HomeController : Controller
{
    
    public HomeController()
    {
    }

    [AllowAnonymous]
    [Route(PagePath.SignedOut)]
    public IActionResult SignedOut()
    {
        HttpContext.Session.Clear();
        return View();
    }

    [HttpGet]
    [Route(PagePath.UserAlreadyExists)]
    public IActionResult UserAlreadyExists()
    {
        return View();
    }
}

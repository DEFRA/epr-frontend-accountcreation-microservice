﻿using FrontendAccountCreation.Web.Constants;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.Controllers.Culture;

[AllowAnonymous]
public class CultureController : Controller
{
    [HttpGet]
    [Route(PagePath.Culture)]
    public LocalRedirectResult UpdateCulture(string culture, string returnUrl)
    {
        HttpContext.Session.SetString(Language.SessionLanguageKey, culture);
        return LocalRedirect(returnUrl);
    }
}

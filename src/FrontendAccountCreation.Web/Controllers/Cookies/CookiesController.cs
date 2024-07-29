using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Cookies;
using FrontendAccountCreation.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Controllers.Cookies;

[ExcludeFromCodeCoverage(Justification = "This controller is covered by integration tests.")]
[AllowAnonymous]
public class CookiesController : Controller
{
    private readonly ICookieService _cookieService;

    public CookiesController(
        ICookieService cookieService)
    {
        _cookieService = cookieService;
    }

    [HttpPost]
    [Route(PagePath.UpdateCookieAcceptance)]
    public LocalRedirectResult UpdateAcceptance(string returnUrl, string cookies)
    {
        _cookieService.SetCookieAcceptance(cookies == CookieAcceptance.Accept, Request.Cookies, Response.Cookies);
        TempData[CookieAcceptance.CookieAcknowledgement] = cookies;

        return LocalRedirect(returnUrl);
    }

    [HttpPost]
    [Route(PagePath.AcknowledgeCookieAcceptance)]
    public LocalRedirectResult AcknowledgeAcceptance(string returnUrl)
    {
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.HomePath();
        }

        return LocalRedirect(returnUrl);
    }
}

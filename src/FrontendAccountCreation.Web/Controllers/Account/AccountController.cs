using FrontendAccountCreation.Web.Controllers.Home;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Controllers.Account
{
    /// <summary>
    /// Controller used in web apps to manage accounts.
    /// </summary>
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        /// <summary>
        /// Handles user sign in.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <param name="redirectUri">Redirect URI.</param>
        /// <returns>Challenge generating a redirect to Azure AD to sign in the user.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignIn(
            [FromRoute] string? scheme,
            [FromQuery] string redirectUri)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            string redirect;
            if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
            {
                redirect = Url.Content(redirectUri);
            }
            else
            {
                redirect = Url.Content("~/");
            }

            return Challenge(
                new AuthenticationProperties { RedirectUri = redirect },
                scheme);
        }

        /// <summary>
        /// Handles the user sign-out.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <returns>Sign out result.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignOut(
            [FromRoute] string? scheme,
            bool? reEx)
        {
            if (AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled)
            {
                return AppServicesAuthenticationInformation.LogoutUrl != null
                    ? LocalRedirect(AppServicesAuthenticationInformation.LogoutUrl)
                    : Ok();
            }

            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            var callbackUrl = Url.Action(action: "SignedOut", controller: "Home", values: new { reEx }, protocol: Request.Scheme);
            return SignOut(
                 new AuthenticationProperties
                 {
                     RedirectUri = callbackUrl,
                 },
                 CookieAuthenticationDefaults.AuthenticationScheme,
                 scheme);

        }

        /// <summary>
        /// Handles the session timeout sign-out.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <returns>Session Timeout Sign out result.</returns>
        [ExcludeFromCodeCoverage(Justification = "Unable to mock authentication")]
        [HttpGet("{scheme?}")]
        [AllowAnonymous]
        public IActionResult SessionSignOut([FromRoute] string? scheme,
            bool? reEx)
        {
            if (AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled)
            {
                if (AppServicesAuthenticationInformation.LogoutUrl != null)
                {
                    return LocalRedirect(AppServicesAuthenticationInformation.LogoutUrl);
                }

                return Ok();
            }

            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            var callbackUrl = Url.Action(action: "TimeoutSignedOut", controller: "Home", values: new { reEx }, protocol: Request.Scheme);

            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = callbackUrl,
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                scheme);
        }
    }
}

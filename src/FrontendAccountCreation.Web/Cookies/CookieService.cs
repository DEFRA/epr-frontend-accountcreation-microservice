﻿using FrontendAccountCreation.Web.Configs;

using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FrontendAccountCreation.Web.Cookies;

public class CookieService : ICookieService
{
    private readonly ILogger<CookieService> _logger;
    private readonly EprCookieOptions _eprCookieOptions;
    private readonly AnalyticsOptions _googleAnalyticsOptions;

    public CookieService(
        ILogger<CookieService> logger,
        IOptions<EprCookieOptions> eprCookieOptions,
        IOptions<AnalyticsOptions> googleAnalyticsOptions)
    {
        _logger = logger;
        _eprCookieOptions = eprCookieOptions.Value;
        _googleAnalyticsOptions = googleAnalyticsOptions.Value;
    }

    public void SetCookieAcceptance(bool accept, IRequestCookieCollection cookies, IResponseCookies responseCookies)
    {
        try
        {
            if (!accept)
            {
                var existingCookies = cookies?.Where(c => c.Key.StartsWith(_googleAnalyticsOptions.CookiePrefix)).ToList();

                if (existingCookies != null)
                {
                    foreach (var cookie in existingCookies)
                    {
                        responseCookies.Append(
                            key: cookie.Key,
                            value: cookie.Value,
                            options: new CookieOptions()
                            {
                                Expires = DateTimeOffset.UtcNow.AddYears(-1),
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                            });
                    }
                }
            }

            var cookieName = _eprCookieOptions.CookiePolicyCookieName;
            ArgumentNullException.ThrowIfNull(cookieName);

            responseCookies.Append(
                key: cookieName,
                value: accept.ToString(),
                options: new CookieOptions()
                {
                    Expires = DateTimeOffset.UtcNow.AddMonths(_eprCookieOptions.CookiePolicyDurationInMonths),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cookie acceptance to '{S}'", accept.ToString());
            throw;
        }
    }

    public bool HasUserAcceptedCookies(IRequestCookieCollection cookies)
    {
        bool cookieAcceptedResult;
        try
        {
            var cookieName = _eprCookieOptions.CookiePolicyCookieName;
            ArgumentNullException.ThrowIfNull(cookieName);

            var cookie = cookies[cookieName];
            cookieAcceptedResult = bool.TryParse(cookie, out bool cookieAccepted) && cookieAccepted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cookie acceptance");
            throw;
        }

        return cookieAcceptedResult;
    }
}
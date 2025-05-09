using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;

using Microsoft.AspNetCore.Http.Features;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FrontendAccountCreation.Web.Middleware;

public class JourneyAccessCheckerMiddleware
{
    private readonly RequestDelegate _next;

    public JourneyAccessCheckerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, ISessionManager<AccountCreationSession> sessionManager)
    {
        var endpoint = httpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<JourneyAccessAttribute>();

        if (attribute != null)
        {
            var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);

            if (attribute.PagePath == PagePath.BusinessAddress && !sessionValue.Journey.Contains(PagePath.BusinessAddress))
            {
                sessionValue.Journey.Add(PagePath.BusinessAddress);
            }

            string? pageToRedirect = null;

            if (sessionValue == null)
            {
                pageToRedirect = PagePath.RegisteredAsCharity;
            }
            else if (sessionValue.Journey.Count == 0)
            {
                //to-do: redirecting to "PageNotFound" only shows the "page not found" page, as "PageNotFound" doesn't exist!
                pageToRedirect = PagePath.PageNotFound;
            }
            else if (!sessionValue.Journey.Contains(attribute.PagePath) && !sessionValue.IsUserChangingDetails)
            {
                pageToRedirect = sessionValue.Journey[sessionValue.Journey.Count - 1];
            }

            if (!string.IsNullOrEmpty(pageToRedirect))
            {
                httpContext.Response.Redirect(pageToRedirect);

                return;
            }
        }

        await _next(httpContext);
    }
}

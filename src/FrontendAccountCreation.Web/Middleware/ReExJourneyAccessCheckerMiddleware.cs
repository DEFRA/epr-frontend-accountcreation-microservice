using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http.Features;

namespace FrontendAccountCreation.Web.Middleware;

public class ReExJourneyAccessCheckerMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext, ISessionManager<ReExAccountCreationSession> sessionManager)
    {
        var endpoint = httpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<ReprocessorExporterJourneyAccessAttribute>();

        if (attribute != null)
        {
            var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);

            string? pageToRedirect = null;

            if (sessionValue == null)
            {
                pageToRedirect = PagePath.FullName;
            }
            else if (sessionValue.Journey.Count == 0)
            {
                pageToRedirect = PagePath.PageNotFoundReEx;
            }
            else if (!sessionValue.Journey.Contains(attribute.PagePath))
            {
                pageToRedirect = sessionValue.Journey[^1];
            }

            if (!string.IsNullOrEmpty(pageToRedirect))
            {
                httpContext.Response.Redirect(pageToRedirect);
                return;
            }
        }

        await next(httpContext);
    }
}

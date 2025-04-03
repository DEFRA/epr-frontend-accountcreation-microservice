﻿using FrontendAccountCreation.Core.Sessions;
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

        //todo: this just loops as there's no session created before the full name page is reached
        // check where account creation session is created
        if (attribute != null)
        {
            var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);

            string? pageToRedirect = null;

            if (sessionValue == null)
            {
                pageToRedirect = PagePath.ReExAccountFullName;
            }
            else if (sessionValue.Journey.Count == 0)
            {
                pageToRedirect = PagePath.PageNotFound;
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

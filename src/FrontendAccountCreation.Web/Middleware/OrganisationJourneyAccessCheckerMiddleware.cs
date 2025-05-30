using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http.Features;

namespace FrontendAccountCreation.Web.Middleware;

public class OrganisationJourneyAccessCheckerMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext, ISessionManager<OrganisationSession> sessionManager)
    {
        var endpoint = httpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<OrganisationJourneyAccessAttribute>();

        if (attribute != null)
        {
            var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);

            string? pageToRedirect = null;

            if (sessionValue == null)
            {
                pageToRedirect = PagePath.RegisteredAsCharity;
            }
            else if (sessionValue.Journey.Count == 0)
            {
                pageToRedirect = PagePath.PageNotFoundReEx;
            }
            else if (!sessionValue.Journey.Contains(attribute.PagePath))
            {
                if (attribute.PagePath == PagePath.TeamMemberRoleInOrganisation ||
                    attribute.PagePath == PagePath.MemberPartnership)
                {
                    var index = sessionValue.Journey.FindIndex(x => x == attribute.PagePath);
                    if (index != -1)
                    {
                        pageToRedirect = sessionValue.Journey[^1];
                    }
                    else
                    {
                        sessionValue.Journey.AddIfNotExists(attribute.PagePath);
                        await sessionManager.SaveSessionAsync(httpContext.Session, sessionValue);
                        pageToRedirect = attribute.PagePath;
                    }
                }
                else
                {
                    pageToRedirect = sessionValue.Journey[^1];
                }
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
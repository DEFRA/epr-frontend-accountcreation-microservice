using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.FeatureManagement;

namespace FrontendAccountCreation.Web.Middleware;

public class OrganisationJourneyAccessCheckerMiddleware(RequestDelegate next, IFeatureManager featureManager)
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
            else if (sessionValue.Journey.Count == 0) // || !await IsPageEnabled(attribute))
            {
                pageToRedirect = PagePath.PageNotFound;
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

    //private async Task<bool> IsPageEnabled(OrganisationJourneyAccessAttribute attribute)
    //{
    //    if (attribute.RequiredFeature == null)
    //    {
    //        return true;
    //    }
    //    return await featureManager.IsEnabledAsync(attribute.RequiredFeature);
    //}
}
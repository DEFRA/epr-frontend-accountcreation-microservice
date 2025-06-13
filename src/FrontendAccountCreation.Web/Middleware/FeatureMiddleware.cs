using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.FeatureManagement;

namespace FrontendAccountCreation.Web.Middleware;

public class FeatureMiddleware(RequestDelegate next, IFeatureManager featureManager)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var attribute = httpContext.Features.Get<IEndpointFeature>()?.Endpoint?.Metadata.GetMetadata<FeatureAttribute>();

        if (!await IsPageEnabled(attribute))
        {
            httpContext.Response.Redirect(PagePath.PageNotFoundReEx);

            return;
        }

        await next(httpContext);
    }

    private async Task<bool> IsPageEnabled(FeatureAttribute? attribute)
    {
        if (attribute?.RequiredFeature == null)
        {
            return true;
        }
        return await featureManager.IsEnabledAsync(attribute.RequiredFeature);
    }
}
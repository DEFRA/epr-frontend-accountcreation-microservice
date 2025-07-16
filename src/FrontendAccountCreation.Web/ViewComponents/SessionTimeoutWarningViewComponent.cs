namespace FrontendAccountCreation.UI.ViewComponents;

using FrontendAccountCreation.UI.ViewModels.Shared;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.UI.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SessionTimeoutWarningViewComponent : ViewComponent
{
    private readonly IFeatureManager _featureManager;

    public SessionTimeoutWarningViewComponent(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var sessionTimeoutWarningModel = new SessionTimeoutWarningModel
        {
           ShowSessionTimeoutWarning = await _featureManager.IsEnabledAsync(FeatureFlags.ShowSessionTimeoutWarning)
        };

        return View(sessionTimeoutWarningModel);
    }
}
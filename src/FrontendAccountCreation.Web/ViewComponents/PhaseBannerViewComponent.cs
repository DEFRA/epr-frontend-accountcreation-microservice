﻿using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Web.ViewComponents;

public class PhaseBannerViewComponent : ViewComponent
{
    private readonly PhaseBannerOptions _bannerOptions;

    public PhaseBannerViewComponent(IOptions<PhaseBannerOptions> bannerOptions)
    {
        _bannerOptions = bannerOptions.Value;
    }

    public ViewViewComponentResult Invoke()
    {
        const string phaseBanner = "PhaseBanner";

        var phaseBannerModel = new PhaseBannerModel()
        {
            Status = $"{phaseBanner}.{_bannerOptions!.ApplicationStatus}",
            Url = _bannerOptions!.SurveyUrl,
            ShowBanner = _bannerOptions!.Enabled
        };
        return View(phaseBannerModel);
    }
}
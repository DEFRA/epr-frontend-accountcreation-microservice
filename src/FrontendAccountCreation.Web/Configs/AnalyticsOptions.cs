﻿namespace FrontendAccountCreation.Web.Configs;

public class AnalyticsOptions
{
    public const string ConfigSection = "GoogleAnalytics";

    public string CookiePrefix { get; set; }

    public string MeasurementId { get; set; }

    public string TagManagerContainerId { get; set; }

    public string DefaultCookieName => CookiePrefix;

    public string AdditionalCookieName => $"{CookiePrefix}_{MeasurementId}";
}

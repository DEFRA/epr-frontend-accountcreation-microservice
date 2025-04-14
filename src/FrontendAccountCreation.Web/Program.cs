using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Extensions;
using FrontendAccountCreation.Web.HealthChecks;
using FrontendAccountCreation.Web.Middleware;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFeatureManagement();

builder.Services
    .RegisterCoreComponents(builder.Configuration)
    .RegisterWebComponents(builder.Configuration)
    .ConfigureMsalDistributedTokenOptions(builder.Configuration);

builder.Services
    .AddAntiforgery(options => {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Name = builder.Configuration.GetValue<string>("CookieOptions:AntiForgeryCookieName");
    })
    .AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddViewLocalization(options =>
    {
        var deploymentRole = builder.Configuration
            .GetValue<string>(DeploymentRoleOptions.ConfigSection);

        options.ResourcesPath = deploymentRole != DeploymentRoleOptions.RegulatorRoleValue 
            ? "Resources" 
            : "ResourcesRegulator";
    })
    .AddDataAnnotationsLocalization();

builder.Services.Configure<DeploymentRoleOptions>(options =>
{
    options.DeploymentRole = builder.Configuration.GetValue<string>(DeploymentRoleOptions.ConfigSection);
});

builder.Services.AddRazorPages();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
    options.ForwardedHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:ForwardedHostHeaderName");
    options.OriginalHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:OriginalHostHeaderName");
    options.AllowedHosts = builder.Configuration.GetValue<string>("ForwardedHeaders:AllowedHosts").Split(";");
});

builder.Services
    .AddApplicationInsightsTelemetry()
    .AddHealthChecks();

builder.Services.AddHsts(options =>
{
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.HttpOnly =  Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

var app = builder.Build();

app.UsePathBase(builder.Configuration.GetValue<string>("PATH_BASE"));

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseForwardedHeaders();

app.UseMiddleware<SecurityHeaderMiddleware>();
app.UseCookiePolicy();
app.UseSession();
app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestLocalization();
app.UseMiddleware<JourneyAccessCheckerMiddleware>();
//todo: will need this, but will need to fix infinite loop
app.UseMiddleware<ReExJourneyAccessCheckerMiddleware>();
//app.UseMiddleware<OrganisationJourneyAccessCheckerMiddleware>();
app.UseMiddleware<AnalyticsCookieMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AccountCreation}/{action=RegisteredAsCharity}/{id?}");

app.MapHealthChecks(
    builder.Configuration.GetValue<string>("HealthCheckPath"),
    HealthCheckOptionBuilder.Build()).AllowAnonymous();

app.MapRazorPages();

app.Run();

namespace FrontendAccountCreation
{
    public partial class Program
    {
    }
}

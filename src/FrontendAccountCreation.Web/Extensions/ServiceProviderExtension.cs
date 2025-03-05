using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Cookies;
using FrontendAccountCreation.Web.Sessions;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Extensions;

[ExcludeFromCodeCoverage(Justification = "This is essentially a config file in code")]
public static class ServiceProviderExtension
{

    public static IServiceCollection RegisterWebComponents(this IServiceCollection services, IConfiguration configuration)
    {
        SetTempDataCookieOptions(services, configuration);
        ConfigureOptions(services, configuration);
        ConfigureLocalization(services);
        ConfigureAuthentication(services, configuration);
        ConfigureAuthorization(services);
        ConfigureSession(services, configuration);
        RegisterServices(services);


        return services;
    }
    
    public static IServiceCollection ConfigureMsalDistributedTokenOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights());
        var buildLogger = loggerFactory.CreateLogger<Program>();

        services.Configure<MsalDistributedTokenCacheAdapterOptions>(options =>
        {
            options.DisableL1Cache = configuration.GetValue("MsalOptions:DisableL1Cache", true);
            options.SlidingExpiration = TimeSpan.FromMinutes(configuration.GetValue("MsalOptions:L2SlidingExpiration", 20));

            options.OnL2CacheFailure = exception =>
            {
                if (exception is RedisConnectionException)
                {
                    buildLogger.LogError(exception, "L2 Cache Failure Redis connection exception: {message}", exception.Message);
                    return true;
                }

                buildLogger.LogError(exception, "L2 Cache Failure: {message}", exception.Message);
                return false;
            };
        });
        return services;
    }

    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EprCookieOptions>(configuration.GetSection(EprCookieOptions.ConfigSection));
        services.Configure<AnalyticsOptions>(configuration.GetSection(AnalyticsOptions.ConfigSection));
        services.Configure<PhaseBannerOptions>(configuration.GetSection(PhaseBannerOptions.ConfigSection));
        services.Configure<ExternalUrlsOptions>(configuration.GetSection(ExternalUrlsOptions.ConfigSection));
        services.Configure<EmailAddressOptions>(configuration.GetSection(EmailAddressOptions.ConfigSection));
        services.Configure<SiteDateOptions>(configuration.GetSection(SiteDateOptions.ConfigSection));
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICookieService, CookieService>();
        services.AddScoped<ISessionManager<AccountCreationSession>, AccountCreationSessionManager>();
    }

    private static void SetTempDataCookieOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CookieTempDataProviderOptions>(options =>
        {
            options.Cookie.Name = configuration.GetValue<string>("CookieOptions:TempDataCookie");
            options.Cookie.Path = configuration.GetValue<string>("PATH_BASE");
        });
    }

    private static void ConfigureLocalization(IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources")
            .Configure<RequestLocalizationOptions>(options =>
            {
                var cultureList = new[] { Language.English, Language.Welsh };
                options.SetDefaultCulture(Language.English);
                options.AddSupportedCultures(cultureList);
                options.AddSupportedUICultures(cultureList);
                options.RequestCultureProviders = new IRequestCultureProvider[]
                {
                    new SessionRequestCultureProvider()
                };
            });
    }

    private static void ConfigureSession(IServiceCollection services, IConfiguration configuration)
    {
        var useLocalSession = configuration.GetValue<bool>("UseLocalSession");

        if (!useLocalSession)
        {

            var redisConnection = configuration.GetConnectionString("REDIS_CONNECTION");

            services.AddDataProtection()
                .SetApplicationName("EprProducers")
                .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnection), "DataProtection-Keys");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = configuration.GetValue<string>("RedisInstanceName");
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSession(options =>
        {
            options.Cookie.Name = configuration.GetValue<string>("CookieOptions:SessionCookieName");
            options.IdleTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionIdleTimeOutMinutes"));
            options.Cookie.IsEssential = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                configuration.GetSection("AzureAdB2C").Bind(options);               
                options.ErrorPath = "/auth-error";
                options.Events.OnRemoteFailure = context =>
                {
                    // Workaround to preserve invitation link if user cancels creating account in Azure AD B2C
                    if (context.Failure.Message.Contains("AADB2C90091:") &&
                        (context.Request.PathBase.HasValue
                            ? context.Properties.RedirectUri.StartsWith($"{context.Request.PathBase}/{PagePath.Invitation}/")
                            : context.Properties.RedirectUri.StartsWith($"/{PagePath.Invitation}/")))
                    {
                        context.Response.Redirect(context.Properties.RedirectUri);
                    }

                    return Task.CompletedTask;
                };
                options.ClaimActions.Add(new CorrelationClaimAction());
            }, options =>
            {
                options.Cookie.Name = configuration.GetValue<string>("CookieOptions:AuthenticationCookieName");
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue<int>("CookieOptions:AuthenticationExpiryInMinutes"));
                options.SlidingExpiration = true;
                options.Cookie.Path = "/";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .EnableTokenAcquisitionToCallDownstreamApi(new string[] {configuration.GetValue<string>("FacadeAPI:DownstreamScope")})
            .AddDistributedTokenCaches();
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}

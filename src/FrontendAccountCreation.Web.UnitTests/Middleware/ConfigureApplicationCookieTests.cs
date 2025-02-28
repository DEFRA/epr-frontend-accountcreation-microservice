using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Web.UnitTests.Middleware;

[TestClass]
public class ConfigureApplicationCookieTests
{

    [TestMethod]
    public void ConfigureApplicationCookie_ShouldSetCorrectOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
        });

        services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var options = optionsMonitor.Get(CookieAuthenticationDefaults.AuthenticationScheme);

        // Assert
        options.Cookie.HttpOnly.Should().BeTrue();
        options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always);
    }

    [TestMethod]
    public void ConfigureApplicationCookie_AntiforgeryCookie_ShouldSetCorrectOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "AntiForgeryCookie";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;

        // Assert
        options.Cookie.HttpOnly.Should().BeTrue();
        options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always);
    }

    [TestMethod]
    public void ConfigureApplicationCookie_TempDataCookie_ShouldSetCorrectOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<CookieTempDataProviderOptions>(options =>
        {
            options.Cookie.Name = "TempDataCookie";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CookieTempDataProviderOptions>>().Value;

        // Assert
        options.Cookie.HttpOnly.Should().BeTrue();
        options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always);
    }

    [TestMethod]
    public void ConfigureApplicationCookie_SessionCookie_ShouldSetCorrectOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSession(options =>
        {
            options.Cookie.Name = "SessionCookie";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SessionOptions>>().Value;

        // Assert
        options.Cookie.HttpOnly.Should().BeTrue();
        options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always);
    }

    [TestMethod]
    public void ConfigureApplicationCookie_AuthenticationCookie_ShouldSetCorrectOptionsA()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddCookie();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "AuthenticationCookie";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
        });

        services.PostConfigure<CookieAuthenticationOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "AuthenticationCookie";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var options = optionsMonitor.Get(OpenIdConnectDefaults.AuthenticationScheme);

        // Assert
        options.Cookie.HttpOnly.Should().BeTrue();
        options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always);
    }
}

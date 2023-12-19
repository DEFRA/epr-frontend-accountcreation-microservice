using Microsoft.AspNetCore.Mvc.Testing;

namespace FrontendAccountCreation.IntegrationTests;

public class TestBase
{
    protected readonly HttpClient _httpClient;

    protected TestBase()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("https_port", "8000");
                builder.UseSetting("ByPassSessionValidation", "true");
            });

        _httpClient = application.CreateClient();
    }
}
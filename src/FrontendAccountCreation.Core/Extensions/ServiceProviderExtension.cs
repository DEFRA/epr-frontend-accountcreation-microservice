﻿using FrontendAccountCreation.Core.Services;
using FrontendAccountCreation.Core.Services.Dto.Company;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrontendAccountCreation.Core.Extensions;

public static class ServiceProviderExtension
{
    public static IServiceCollection RegisterCoreComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ComplianceSchemeOptions>(configuration.GetSection(ComplianceSchemeOptions.ConfigSection));

        var useMockData = configuration.GetValue<bool>("FacadeAPI:UseMockData");
        if (useMockData)
        {
            services.AddSingleton<IFacadeService, MockedFacadeService>();
        }
        else
        {
            services.AddHttpClient<IFacadeService, FacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")));
        }

        services.AddSingleton<ICompanyService, CompanyService>();
        services.AddSingleton<IAccountMapper, AccountMapper>();

        return services;
    }
}

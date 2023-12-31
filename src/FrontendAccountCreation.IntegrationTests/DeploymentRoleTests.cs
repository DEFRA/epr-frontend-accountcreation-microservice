﻿using FluentAssertions;
using FrontendAccountCreation.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace FrontendAccountCreation.IntegrationTests;

[TestClass]
public class DeploymentRoleTests
{
    [TestMethod]
    [DataRow("Regulator", "pEPR : Regulators’ Service")]
    [DataRow("Producer", "Report packaging data")]
    [DataRow(null, "Report packaging data")]
    [DataRow("Anything except Regulator", "Report packaging data")]
    [DataRow("RRegulator", "Report packaging data")]
    [DataRow("Regulatorr", "Report packaging data")]
    [DataRow("Anything except Regulator", "Report packaging data")]
    public async Task Returned_app_title_depends_on_deployment_role(string deploymentRole, string expectedApplication)
    {
        // Arrange
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ByPassSessionValidation", "true");
                builder.UseSetting("UseLocalSession", "true");
                builder.UseSetting("DeploymentRole", deploymentRole);
            });

        // Act
        var local = application.Services.GetService<IStringLocalizer<SharedResources>>();

        // Assert
        local["ApplicationTitle"].ToString().Should().Be(expectedApplication);
    }
}
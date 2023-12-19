using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.Extensions.Options;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.ViewModels;

[TestClass]
public class FullNameViewModelTests
{
    [TestMethod]
    [DataRow("John", "Smith", DeploymentRoleOptions.RegulatorRoleValue, 0)]
    [DataRow("John", "Smith", "Producer", 0)]
    [DataRow(null, "Smith", null, 1)]
    [DataRow(null, null, null, 2)]
    [DataRow("J0hn", "Sm1th", null, 0)]
    [DataRow("J0hn", "Sm1th", DeploymentRoleOptions.RegulatorRoleValue, 2)]
    [DataRow("JohnJohnJohnJohnJohnJohnJohnJohnJohn", "Smith", "Producer", 0)]
    [DataRow("JohnJohnJohnJohnJohnJohnJohnJohnJohn", "Smith", DeploymentRoleOptions.RegulatorRoleValue, 1)]
    [DataRow("John", "SmithSmithSmithSmithSmithSmithSmithSmithSmithSmithSmith", DeploymentRoleOptions.RegulatorRoleValue, 1)]
    [DataRow("John", "SmithSmithSmithSmithSmithSmithSmithSmithSmithSmithSmith", null, 1)]
    [DataRow(" ", "Smith", DeploymentRoleOptions.RegulatorRoleValue, 1)]
    [DataRow("Smith", " ", DeploymentRoleOptions.RegulatorRoleValue, 1)]
    [DataRow("JohnJohnJohnJohnJohnJohnJohnJohnJohnJohnJohnJohnAAA", "Smith", "Producer", 1)]
    public void Validate_producers(string? firstName, string? lastName, string? deploymentRole, int expectedNumberOfErrors)
    {
        // Arrange
        var vm = new FullNameViewModel{FirstName = firstName, LastName = lastName};
        var mockService = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<DeploymentRoleOptions>>();
        var deploymentRoleOptions = new DeploymentRoleOptions{ DeploymentRole = deploymentRole };
        mockService.Setup(m => m.GetService(typeof(IOptions<DeploymentRoleOptions>))).Returns(mockOptions.Object);
        mockOptions.Setup(m => m.Value).Returns(deploymentRoleOptions);

        // Act
        var validationResults = vm.Validate(new ValidationContext(vm, mockService.Object, null));

        // Assert
        validationResults.Count().Should().Be(expectedNumberOfErrors);
    }
}
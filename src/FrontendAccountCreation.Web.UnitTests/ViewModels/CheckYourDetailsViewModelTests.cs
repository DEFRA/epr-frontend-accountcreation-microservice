using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace FrontendAccountCreation.Web.UnitTests.ViewModels.AccountCreation;

[TestClass]
public class CheckYourDetailsViewModelTests
{
    [DataTestMethod]
    [DataRow(ProducerType.NonUkOrganisation, "CheckYourDetails.NonUkOrganisation")]
    [DataRow(ProducerType.Partnership, "CheckYourDetails.Partnership")]
    [DataRow(ProducerType.SoleTrader, "CheckYourDetails.SoleTrader")]
    [DataRow(ProducerType.UnincorporatedBody, "CheckYourDetails.UnincorporatedBody")]
    [DataRow(ProducerType.Other, "CheckYourDetails.Other")]
    [DataRow(ProducerType.NotSet, "")]
    public void TypeOfProducer_ShouldReturnCorrectValue(ProducerType producerType, string expected)
    {
        // Arrange
        var viewModel = new CheckYourDetailsViewModel
        {
            ProducerType = producerType
        };

        // Act
        var result = viewModel.TypeOfProducer;

        // Assert
        result.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(RoleInOrganisation.Director, "CheckYourDetails.Director")]
    [DataRow(RoleInOrganisation.CompanySecretary, "CheckYourDetails.CompanySecretary")]
    [DataRow(RoleInOrganisation.Partner, "CheckYourDetails.Partner")]
    [DataRow(RoleInOrganisation.Member, "CheckYourDetails.Member")]
    [DataRow(RoleInOrganisation.NoneOfTheAbove, "")]
    public void YourRole_ShouldReturnCorrectValue(RoleInOrganisation roleInOrganisation, string expected)
    {
        // Arrange
        var viewModel = new CheckYourDetailsViewModel
        {
            RoleInOrganisation = roleInOrganisation
        };

        // Act
        var result = viewModel.YourRole;

        // Assert
        result.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(Nation.England, "CheckYourDetails.England")]
    [DataRow(Nation.Scotland, "CheckYourDetails.Scotland")]
    [DataRow(Nation.Wales, "CheckYourDetails.Wales")]
    [DataRow(Nation.NorthernIreland, "CheckYourDetails.NorthernIreland")]
    [DataRow(Nation.NotSet, "")]
    public void UkNation_ShouldReturnCorrectValue(Nation nation, string expected)
    {
        // Arrange
        var viewModel = new CheckYourDetailsViewModel
        {
            Nation = nation
        };

        // Act
        var result = viewModel.UkNation;

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var viewModel = new CheckYourDetailsViewModel();

        // Assert
        viewModel.OrganisationType.Should().Be(OrganisationType.NotSet);
        viewModel.ProducerType.Should().Be(ProducerType.NotSet);
        viewModel.RoleInOrganisation.Should().Be(RoleInOrganisation.NoneOfTheAbove);
        viewModel.Nation.Should().Be(Nation.NotSet);
        viewModel.CompanyName.Should().BeNull();
        viewModel.CompaniesHouseNumber.Should().BeNull();
        viewModel.TradingName.Should().BeNull();
        viewModel.BusinessAddress.Should().BeNull();
        viewModel.YourFullName.Should().BeNull();
        viewModel.TelephoneNumber.Should().BeNull();
        viewModel.IsCompaniesHouseFlow.Should().BeFalse();
        viewModel.IsComplianceScheme.Should().BeFalse();
        viewModel.IsManualInputFlow.Should().BeFalse();
        viewModel.JobTitle.Should().BeNull();
    }
}

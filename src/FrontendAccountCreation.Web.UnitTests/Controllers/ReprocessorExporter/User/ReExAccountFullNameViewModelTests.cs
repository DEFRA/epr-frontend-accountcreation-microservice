namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.User;

using FluentAssertions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Moq;

[TestClass]
public class ReExAccountFullNameViewModelTests
{
    [TestMethod]
    [DataRow("John", "Smith", 0)]
    [DataRow(null, "Smith", 1)]
    [DataRow(null, null, 2)]
    [DataRow("John12345566789012132232425262728293031323334", "Smith", 2)]
    [DataRow("John", "Smith12345566789012132232425262728293031323334", 2)]
    public void Validate_fullname(string? firstName, string? lastName, int expectedNumberOfErrors)
    {
        // Arrange
        var vm = new ReExAccountFullNameViewModel{FirstName = firstName, LastName = lastName};
        var mockService = new Mock<IServiceProvider>();

        // Act
        var validationResults = vm.Validate(new ValidationContext(vm, mockService.Object, null));

        // Assert
        validationResults.Count().Should().Be(expectedNumberOfErrors);
    }
}
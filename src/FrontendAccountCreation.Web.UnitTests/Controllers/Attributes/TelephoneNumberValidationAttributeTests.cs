using FrontendAccountCreation.Web.Controllers.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Attributes;

[TestClass]
public class TelephoneNumberValidationAttributeTests
{
    [TestMethod]
    public void IsValid_ReturnsSuccess_WhenPhoneNumberIsValid()
    {
        // Arrange
        var attribute = new TelephoneNumberValidationAttribute();
        var context = new ValidationContext(new object());
        string validPhoneNumber = "+44 123 456 7890";

        // Act
        var result = attribute.GetValidationResult(validPhoneNumber, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [TestMethod]
    public void IsValid_ReturnsValidationResult_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        var attribute = new TelephoneNumberValidationAttribute { ErrorMessage = "Invalid phone number" };
        var context = new ValidationContext(new object());
        string invalidPhoneNumber = "invalid-phone-number";

        // Act
        var result = attribute.GetValidationResult(invalidPhoneNumber, context);

        // Assert
        result.Should().NotBeNull();
        result.ErrorMessage.Should().Be("Invalid phone number");
    }

    [TestMethod]
    public void IsValid_ReturnsValidationResult_WhenPhoneNumberIsNull()
    {
        // Arrange
        var attribute = new TelephoneNumberValidationAttribute { ErrorMessage = "Phone number is required" };
        var context = new ValidationContext(new object());

        // Act
        var result = attribute.GetValidationResult(null, context);

        // Assert
        result.Should().NotBeNull();
        result.ErrorMessage.Should().Be("Phone number is required");
    }
}

using FluentAssertions;
using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Attributes;

[TestClass]
public class TeamMemberTelephoneNumberValidationAttributeTests
{
    [TestMethod]
    public void IsValid_ReturnsSuccess_WhenPhoneNumberIsValid()
    {
        // Arrange
        var attribute = new TeamMemberTelephoneNumberValidationAttribute();
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
        var attribute = new TeamMemberTelephoneNumberValidationAttribute { ErrorMessage = "Invalid phone number" };
        var context = new ValidationContext(new object());
        string invalidPhoneNumber = "invalid-phone-number";

        // Act
        var result = attribute.GetValidationResult(invalidPhoneNumber, context);

        // Assert
        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Be("Invalid phone number");
    }

    [TestMethod]
    public void IsValid_ReturnsSuccess_WhenPhoneNumberIsNull()
    {
        // Arrange
        var attribute = new TeamMemberTelephoneNumberValidationAttribute { ErrorMessage = "Phone number is required" };
        var context = new ValidationContext(new object());

        // Act
        var result = attribute.GetValidationResult(null, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [TestMethod]
    public void IsValid_ReturnsSuccess_WhenPhoneNumberIsEmpty()
    {
        // Arrange
        var attribute = new TeamMemberTelephoneNumberValidationAttribute { ErrorMessage = "Phone number is required" };
        var context = new ValidationContext(new object());
        string emptyPhoneNumber = "";

        // Act
        var result = attribute.GetValidationResult(emptyPhoneNumber, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }
}
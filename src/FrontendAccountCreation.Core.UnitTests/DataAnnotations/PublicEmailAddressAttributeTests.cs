using FluentAssertions;
using FrontendAccountCreation.Core.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Core.UnitTests.DataAnnotations;

/// <summary>
/// This file contains the unit tests for PublicEmailAddressAttribute.
/// It verifies that the attribute correctly validates various email address formats.
/// </summary>
[TestClass]
public class PublicEmailAddressAttributeTests
{
    /// <summary>
    /// This test method checks a series of valid email addresses that should pass validation.
    /// </summary>
    /// <param name="email">A valid email address to test.</param>
    [DataTestMethod]
    [DataRow("test@example.com")]
    [DataRow("user.name@domain.co.uk")]     // Multi-part TLD
    [DataRow("user@sub.domain.net")]        // Subdomain
    [DataRow("user@example.com.au")]        // Another multi-part TLD
    [DataRow("user+alias@gmail.com")]
    [DataRow("user-name@my-domain.net")]    // Minus signs in the domain are allowed
    [DataRow("user@my-domain-10.net")]      // digits in the domain are allowed
    [DataRow("12345@domain.io")]
    [DataRow("test@domain.c")]              // Single character TLD's are allowed, but one doesn't currently exist (see https://data.iana.org/TLD/tlds-alpha-by-domain.txt)
    public void IsValid_ShouldReturnTrue_ForValidPublicEmails(string email)
    {
        // Arrange
        var attribute = new PublicEmailAddressAttribute();

        // Act
        bool isValid = attribute.IsValid(email);

        // Assert
        isValid.Should().BeTrue($"because '{email}' is a valid public email address");
    }

    /// <summary>
    /// This test method checks various invalid email formats that should fail validation.
    /// This is the key test to ensure emails without a TLD are rejected.
    /// </summary>
    /// <param name="email">An invalid email address to test.</param>
    [DataTestMethod]
    // Emails without a TLD
    [DataRow("test@localhost")]
    [DataRow("user@hostname")]
    // General malformed emails
    [DataRow("test.example.com")]     // Missing @
    [DataRow("test@")]                // Missing domain and TLD
    [DataRow("@example.com")]         // Missing local part
    [DataRow("test@.com")]            // Domain starts with a dot
    [DataRow("test@domain.")]         // TLD is empty
    [DataRow("test @example.com")]    // Contains whitespace
    [DataRow("test@domain,com")]      // Comma in domain
    [DataRow("test@dom_ain.com")]     // Underscore in domain
    [DataRow("test@domain.co,m")]     // Comma in TLD
    [DataRow("test@domain!.com")]     // Exclamation in domain
    [DataRow("test@domain$.com")]     // Dollar sign in domain
    [DataRow("test@domain..com")]     // Consecutive dots
    [DataRow("test@-domain.com")]     // Leading hyphen in domain
    [DataRow("test@domain-.com")]     // Trailing hyphen in domain
    public void IsValid_ShouldReturnFalse_ForInvalidEmails(string email)
    {
        // Arrange
        var attribute = new PublicEmailAddressAttribute();

        // Act
        bool isValid = attribute.IsValid(email);

        // Assert
        isValid.Should().BeFalse($"because '{email}' is an invalid email address");
    }

    /// <summary>
    /// Tests that null, empty, or whitespace strings are considered valid by this attribute.
    /// This allows the [Required] attribute to handle these cases independently.
    /// </summary>
    /// <param name="email">The null or whitespace string to test.</param>
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void IsValid_ShouldReturnTrue_ForNullOrWhitespace(string? email)
    {
        // Arrange
        var attribute = new PublicEmailAddressAttribute();

        // Act
        bool isValid = attribute.IsValid(email);

        // Assert
        isValid.Should().BeTrue("because null or whitespace should be handled by the [Required] attribute");
    }

    /// <summary>
    /// Tests that the IsValid method returns a ValidationResult with the correct error message
    /// when validation fails. This ensures custom error messages are applied correctly.
    /// </summary>
    [TestMethod]
    public void IsValid_ShouldReturnValidationResult_WithCorrectErrorMessage()
    {
        // Arrange
        var model = new TestModel { Email = "invalid-email" };
        var validationContext = new ValidationContext(model) { MemberName = nameof(TestModel.Email) };
        var attribute = new PublicEmailAddressAttribute { ErrorMessage = "This is a custom error message." };

        // Act
        var validationResult = attribute.GetValidationResult(model.Email, validationContext);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.ErrorMessage.Should().Be("This is a custom error message.");
    }

    /// <summary>
    /// A helper class used for testing the validation context and error message generation.
    /// </summary>
    private class TestModel
    {
        public string Email { get; set; } = string.Empty;
    }
}

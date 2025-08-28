using FluentAssertions;
using FrontendAccountCreation.Web.ErrorNext;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.UnitTests.ErrorNext;

[TestClass]
public class ModelStateErrorTests
{
    [TestMethod]
    public void Constructor_WithValidModelStateEntry_SetsPropertiesCorrectly()
    {
        // Arrange
        var key = "TestField";
        var errorMessage = "Test error message";
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(key, errorMessage);
        var kvp = modelState.First();

        // Act
        var error = new ModelStateError(kvp);
        // Sonar is chuntering about not using IError directly for performance reasons,
        // but this is a test for the IError implementation
#pragma warning disable CA1859
        var iError = (IError)error;
#pragma warning restore

        // Assert
        error.Message.Should().Be(errorMessage);
        error.HtmlElementId.Should().Be(key);
        error.InputErrorMessageParaId.Should().Be($"error-message-{key}");
        iError.FormGroupClass.Should().Be("govuk-form-group--error");
        iError.InputClass.Should().Be("govuk-input--error");
        iError.TextAreaClass.Should().Be("govuk-textarea--error");
        iError.SelectClass.Should().Be("govuk-select--error");
    }

    [TestMethod]
    public void Constructor_WithNoErrors_ThrowsInvalidOperationException()
    {
        // Arrange
        var key = "TestField";
        var modelState = new ModelStateDictionary();
        // Add the key with no errors
        modelState.SetModelValue(key, rawValue: null, attemptedValue: null);
        var kvp = new KeyValuePair<string, ModelStateEntry?>(key, modelState[key]);

        // Act
        // now Sonar is complaining about the constructed ModelStateError not being used,
        // but we want to test that it throws an exception
#pragma warning disable CA1806
        Action act = () => new ModelStateError(kvp);
#pragma warning restore

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No errors found in the model state entry.");
    }
}
using FluentAssertions;
using FrontendAccountCreation.Web.ErrorNext;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.UnitTests.ErrorNext;

[TestClass]
public class ErrorStateFromModelStateTests
{
    [TestMethod]
    public void Create_WithEmptyModelState_ReturnsErrorStateEmpty()
    {
        // Arrange
        var modelState = new ModelStateDictionary();

        // Act
        var result = ErrorStateFromModelState.Create(modelState);

        // Assert
        result.Should().BeOfType<ErrorStateEmpty>();
        result.Errors.Should().BeEmpty();
        result.GetErrorIfTriggeredByElementId().Should().BeNull();
    }

    [TestMethod]
    public void Create_WithErrorsInModelState_ReturnsErrorStateFromModelState()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error1");
        modelState.AddModelError("Field2", "Error2");

        // Act
        var result = ErrorStateFromModelState.Create(modelState);

        // Assert
        result.Should().BeOfType<ErrorStateFromModelState>();
        result.Errors.Should().HaveCount(2);
    }

    [TestMethod]
    public void Errors_Property_ReturnsModelStateErrors()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error1");
        modelState.AddModelError("Field2", "Error2");
        var errorState = new ErrorStateFromModelState(modelState);

        // Act
        var errors = errorState.Errors.ToList();

        // Assert
        errors.Should().HaveCount(2);
        errors[0].HtmlElementId.Should().Be("Field1");
        errors[0].Message.Should().Be("Error1");
        errors[1].HtmlElementId.Should().Be("Field2");
        errors[1].Message.Should().Be("Error2");
    }

    [TestMethod]
    public void GetErrorIfTriggeredByElementId_WithNoIds_ReturnsFirstError()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error1");
        modelState.AddModelError("Field2", "Error2");
        var errorState = new ErrorStateFromModelState(modelState);

        // Act
        var error = errorState.GetErrorIfTriggeredByElementId();

        // Assert
        error.Should().NotBeNull();
        error!.HtmlElementId.Should().Be("Field1");
    }

    [TestMethod]
    public void GetErrorIfTriggeredByElementId_WithMatchingId_ReturnsCorrectError()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error1");
        modelState.AddModelError("Field2", "Error2");
        var errorState = new ErrorStateFromModelState(modelState);

        // Act
        var error = errorState.GetErrorIfTriggeredByElementId("Field2");

        // Assert
        error.Should().NotBeNull();
        error!.HtmlElementId.Should().Be("Field2");
        error.Message.Should().Be("Error2");
    }

    [TestMethod]
    public void GetErrorIfTriggeredByElementId_WithNonMatchingId_ReturnsNull()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error1");
        var errorState = new ErrorStateFromModelState(modelState);

        // Act
        var error = errorState.GetErrorIfTriggeredByElementId("NonExistentField");

        // Assert
        error.Should().BeNull();
    }
}
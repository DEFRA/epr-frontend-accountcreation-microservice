using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Web.UnitTests.ViewModels;

[TestClass]
public class IsTradingNameDifferentViewModelTests
{
    /// <summary>
    /// Helper method to perform validation and return the results.
    /// This reduces boilerplate code in each test.
    /// </summary>
    /// <param name="viewModel">The view model instance to validate.</param>
    /// <returns>A list of validation results.</returns>
    private static List<ValidationResult> ValidateViewModel(IsTradingNameDifferentViewModel viewModel)
    {
        var validationContext = new ValidationContext(viewModel);
        return viewModel.Validate(validationContext).ToList();
    }

    [DataTestMethod]
    [DataRow(false, "IsTradingNameDifferent.ErrorMessage", "the standard error message should be shown for UK context")]
    [DataRow(true, "IsTradingNameDifferent.NonUk.ErrorMessage", "the specific non-UK error message should be shown for non-UK context")]
    public void Validate_WhenIsTradingNameDifferentIsNull_ReturnsCorrectError(bool isNonUk, string expectedErrorMessage, string because)
    {
        // Arrange
        var viewModel = new IsTradingNameDifferentViewModel
        {
            IsTradingNameDifferent = null, // The scenario being tested is when the value is null
            IsNonUk = isNonUk
        };

        // Act
        var validationResults = ValidateViewModel(viewModel);

        // Assert
        validationResults.Should().HaveCount(1, because);

        var singleResult = validationResults.Single();
        singleResult.ErrorMessage.Should().Be(expectedErrorMessage);
        singleResult.MemberNames.Should().ContainSingle()
            .Which.Should().Be(nameof(IsTradingNameDifferentViewModel.IsTradingNameDifferent),
                "because the error should always be associated with the correct property");
    }

    [DataTestMethod]
    [DataRow(YesNoAnswer.Yes, false)]
    [DataRow(YesNoAnswer.Yes, true)]
    [DataRow(YesNoAnswer.No, false)]
    [DataRow(YesNoAnswer.No, true)]
    public void Validate_WhenIsTradingNameDifferentHasValue_ReturnsNoErrors(YesNoAnswer providedAnswer, bool isNonUk)
    {
        // Arrange
        var viewModel = new IsTradingNameDifferentViewModel
        {
            IsTradingNameDifferent = providedAnswer,
            IsNonUk = isNonUk
        };

        // Act
        var validationResults = ValidateViewModel(viewModel);

        // Assert
        validationResults.Should().BeEmpty("because a non-null value for the property should always pass validation");
    }
}

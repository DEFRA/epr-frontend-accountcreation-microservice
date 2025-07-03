
namespace FrontendAccountCreation.Web.ErrorNext;

/// <summary>
/// Represents a possible error that is displayed in the error summary and next to the input control.
/// </summary>
/// <param name="Id">The error id (usually the int representation of an enum of error types.)</param>
/// <param name="ErrorMessage">The error message that is displayed in the error summary and next to the input control.</param>
public record PossibleError(int Id, string ErrorMessage);

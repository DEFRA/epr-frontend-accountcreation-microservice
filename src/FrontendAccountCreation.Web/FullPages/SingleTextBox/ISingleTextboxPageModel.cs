using FrontendAccountCreation.Web.ErrorNext;

namespace FrontendAccountCreation.Web.FullPages.SingleTextBox;

public interface ISingleTextboxPageModel
{
    string Question { get; }
    
    string? Hint { get; }

    /// <summary>
    /// Optional label for the text box (in addition to the question).
    /// </summary>
    string? TextBoxLabel { get; }

    string? TextBoxValue { get; }

    string ButtonText => "Continue";

    int? MaxLength { get; }

    IErrorState Errors { get; }
}
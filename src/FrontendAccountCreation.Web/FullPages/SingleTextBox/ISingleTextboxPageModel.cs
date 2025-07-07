using FrontendAccountCreation.Web.ErrorNext;

namespace FrontendAccountCreation.Web.FullPages.SingleTextBox;

public interface ISingleTextboxPageModel
{
    string Question { get; }
    
    ///// <summary>
    ///// Optional separate heading. If not supplied, the heading will be the same as the <see cref="TextBoxLabel"/>.
    ///// </summary>
    //string? Heading { get; }
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
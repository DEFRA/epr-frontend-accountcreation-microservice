using FrontendAccountCreation.Web.ErrorNext;

namespace FrontendAccountCreation.Web.FullPages.SingleTextBox;

public interface ISingleTextboxPageModel
{
    /// <summary>
    /// Optional separate heading. If not supplied, the heading will be the same as the <see cref="TextBoxLabel"/>.
    /// </summary>
    string? Heading { get; }
    string? Hint { get; }
    string TextBoxLabel { get; }
    string? TextBoxValue { get; }
    string ButtonText => "Continue";

    int? MaxLength { get; }

    IErrorState Errors { get; }
}
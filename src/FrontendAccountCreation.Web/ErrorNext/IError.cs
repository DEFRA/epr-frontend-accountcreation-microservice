namespace FrontendAccountCreation.Web.ErrorNext;

public interface IError
{
    int Id { get; }
    string Message { get; }
    string HtmlElementId { get; }

    /// <summary>
    /// The id of the error message HTML element that is displayed next to the input control.
    /// Will be used as the aria-describedby attribute value, when the input is in an errored state.
    /// </summary>
    string InputErrorMessageParaId { get; }

    string FormGroupClass => "govuk-form-group--error";

    string InputClass => "govuk-input--error";
    string TextAreaClass => "govuk-textarea--error";
    string SelectClass => "govuk-select--error";
}

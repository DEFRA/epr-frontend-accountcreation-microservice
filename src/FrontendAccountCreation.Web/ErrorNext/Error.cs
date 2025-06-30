
namespace FrontendAccountCreation.Web.ErrorNext;

public class Error(PossibleError possibleError, ErrorState errorState)
{
    //todo: does this need to be public?
    public int Id => possibleError.Id;
    public string Message => possibleError.ErrorMessage;

    public string HtmlElementId
    {
        get
        {
            if (errorState.ErrorIdToHtmlElementId == null)
            {
                throw new InvalidOperationException($"ErrorIdToHtmlElementId is null. Set it on {nameof(ErrorState)}.");
            }

            return errorState.ErrorIdToHtmlElementId(Id);
        }
    }

    //todo: tag helpers to add extra classes/aria-describedby to input element?

    /// <summary>
    /// The id of the error message HTML element that is displayed next to the input control.
    /// Will be used as the aria-describedby attribute value, when the input is in an errored state.
    /// </summary>
    public string InputErrorMessageParaId => $"{HtmlElementId}-error-message";

    public string FormGroupClass => "govuk-form-group--error";

    public string InputClass => "govuk-input--error";
    public string TextAreaClass => "govuk-textarea--error";
    public string SelectClass => "govuk-select--error";
}
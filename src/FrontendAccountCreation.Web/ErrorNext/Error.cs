using Microsoft.AspNetCore.Mvc.ModelBinding;

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

public class ModelStateError
    : IError
{
    public ModelStateError(KeyValuePair<string, ModelStateEntry?> modelStateEntry)
    {
        //todo:
        Id = 0; // ModelStateError does not have an ID, so we use 0.
        //todo: what to throw?
        //todo: localize here, rather than in the view for better testability
        Message = modelStateEntry.Value.Errors.FirstOrDefault()?.ErrorMessage ?? throw new ArgumentNullException(nameof(modelStateEntry));
        //HtmlElementId = modelStateEntry.Key;
        //todo: store key in error, so can check by htmlelementid instead/as well as id?
        //todo: for radios don't add value to first (if need way to link to first input)
        HtmlElementId = modelStateEntry.Key; //.Replace(".", "_");
        InputErrorMessageParaId = $"error-message-{HtmlElementId}";
    }

    //public string HtmlElementId
    //{
    //    get
    //    {
            
    //        //if (errorState.ErrorIdToHtmlElementId == null)
    //        //{
    //        //    throw new InvalidOperationException($"ErrorIdToHtmlElementId is null. Set it on {nameof(ErrorState)}.");
    //        //}

    //        //return errorState.ErrorIdToHtmlElementId(Id);
    //    }
    //}

    public int Id { get; }
    public string Message { get; }
    public string HtmlElementId { get; }
    public string InputErrorMessageParaId { get; }
}

public class Error(PossibleError possibleError, ErrorState errorState)
    : IError
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

    public string InputErrorMessageParaId => $"{HtmlElementId}-error-message";

    //public string FormGroupClass => "govuk-form-group--error";

    //public string InputClass => "govuk-input--error";
    //public string TextAreaClass => "govuk-textarea--error";
    //public string SelectClass => "govuk-select--error";
}
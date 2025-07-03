using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountCreation.Web.ErrorNext;

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
        HtmlElementId = modelStateEntry.Key;
        InputErrorMessageParaId = $"error-message-{HtmlElementId}";
    }

    public int Id { get; }
    public string Message { get; }
    public string HtmlElementId { get; }
    public string InputErrorMessageParaId { get; }
}

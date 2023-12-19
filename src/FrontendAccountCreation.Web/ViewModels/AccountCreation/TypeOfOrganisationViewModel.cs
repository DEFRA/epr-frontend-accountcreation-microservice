using FrontendAccountCreation.Core.Sessions;

using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class TypeOfOrganisationViewModel
{
    [Required(ErrorMessage = "TypeOfOrganisation.ErrorMessage")]
    public ProducerType? ProducerType { get; set; }
}

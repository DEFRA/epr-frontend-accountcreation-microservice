using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExTypeOfOrganisationViewModel
{
    [Required(ErrorMessage = "ReExTypeOfOrganisation.ErrorMessage")]
    public ProducerType? ProducerType { get; set; }
}

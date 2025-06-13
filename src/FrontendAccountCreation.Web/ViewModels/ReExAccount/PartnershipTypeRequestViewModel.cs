using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class PartnershipTypeRequestViewModel
{
    [Required(ErrorMessage = "PartnershipType.ErrorMessage")]
    public PartnershipType? TypeOfPartnership { get; set; }
}

using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class PartnerDetailsViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "PartnerDetails.EmailError")]
    public string Email { get; set; }

    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TelephoneNumber.TelephoneNumberErrorMessage")]
    public string Telephone { get; set; }

    [Required(ErrorMessage = "PartnerDetails.FirstNameErrorMessage")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "PartnerDetails.LastNameErrorMessage")]
    public string LastName { get; set; }
}
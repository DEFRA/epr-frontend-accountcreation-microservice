using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class DeclarationViewModelWithFullName
{
    [MaxLength(200, ErrorMessage = "DeclarationWithFullName.LengthErrorMessage")]
    [Required(ErrorMessage = "DeclarationWithFullName.ErrorMessage")]
    public string? FullName { get; set; }
}
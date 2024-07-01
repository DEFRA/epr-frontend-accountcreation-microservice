using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class DeclarationViewModelWithFullName
{
    [MaxLength(70, ErrorMessage = "Declaration.LengthErrorMessage")]
    [Required(ErrorMessage = "Declaration.ErrorMessage")]
    public string? FullName { get; set; }
}
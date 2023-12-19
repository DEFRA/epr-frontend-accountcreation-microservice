using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class CompaniesHouseNumberViewModel
{
    [Required(ErrorMessage = "CompaniesHouseNumber.ErrorMessage")]
    [MaxLength(8, ErrorMessage = "CompaniesHouseNumber.LengthErrorMessage")]
    public string? CompaniesHouseNumber { get; set; }
}

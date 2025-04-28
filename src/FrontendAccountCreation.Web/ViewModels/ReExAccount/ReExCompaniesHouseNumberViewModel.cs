using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;
public class ReExCompaniesHouseNumberViewModel
{
    [Required(ErrorMessage = "CompaniesHouseNumber.ErrorMessage")]
    [MaxLength(8, ErrorMessage = "CompaniesHouseNumber.MaxLengthErrorMessage")]
    [MinLength(8, ErrorMessage = "CompaniesHouseNumber.MinLengthErrorMessage")]
    public string? CompaniesHouseNumber { get; set; }
}

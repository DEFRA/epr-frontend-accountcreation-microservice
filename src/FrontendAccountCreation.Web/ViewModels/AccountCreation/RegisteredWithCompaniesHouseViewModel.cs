using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class RegisteredWithCompaniesHouseViewModel
{
    [Required(ErrorMessage = "RegisteredWithCompaniesHouse.ErrorMessage")]
    public YesNoAnswer? IsTheOrganisationRegistered { get; set; }
}

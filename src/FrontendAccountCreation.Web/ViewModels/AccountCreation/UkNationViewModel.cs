using FrontendAccountCreation.Core.Sessions;

using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class UkNationViewModel
{
    [Required]
    public Nation? UkNation { get; set; }


    public bool IsCompaniesHouseFlow { get; set; }

    public bool IsManualInputFlow { get; set; }
}

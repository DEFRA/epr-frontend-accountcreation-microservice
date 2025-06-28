using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Models;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class ManageControlOrganisationViewModel
{
    [Required(ErrorMessage = "ManageControlOrganisation.ErrorMessage")]
    public YesNoNotSure? TheyManageOrControlOrganisationOrInCharge { get; set; }
}

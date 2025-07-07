using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Models;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class ManageControlViewModel
{
    [Required(ErrorMessage = "ManageControl.UserManagesOrControlsError")]
    public YesNoNotSure? UserManagesOrControls { get; set; }
    public bool IsUnincorporatedFlow { get; set; }
}
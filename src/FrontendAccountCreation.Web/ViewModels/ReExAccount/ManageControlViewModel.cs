using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Models;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class ManageControlViewModel
{
    public YesNoNotSure? UserManagesOrControls { get; set; }
}
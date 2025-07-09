using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Sessions.ReEx;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;

[ExcludeFromCodeCoverage]
public class ReExManageControlOrganisationViewModel
{
    [Required(ErrorMessage = "ReExManageControlOrganisation.ErrorMessage.Required")]
    public ManageControlAnswer? Answer { get; set; }
}

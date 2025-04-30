using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class IsOrganisationAPartnerViewModel
{
    [Required(ErrorMessage = "IsOrganisationAPartner.ErrorMessage")]
    public YesNoAnswer? IsOrganisationAPartner { get; set; }
}

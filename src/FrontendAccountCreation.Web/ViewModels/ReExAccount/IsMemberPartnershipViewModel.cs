using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class IsMemberPartnershipViewModel
{
    [Required(ErrorMessage = "MemberPartnership.ErrorMessage")]
    public YesNoAnswer? IsMemberPartnership { get; set; }
    public Guid? Id { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage(Justification = "DTO and doesn't have any logic")]
public class IsMemberPartnershipViewModel
{
    [Required(ErrorMessage = "MemberPartnership.ErrorMessage")]
    public YesNoAnswer? IsMemberPartnership { get; set; }
    public Guid? Id { get; set; }
}
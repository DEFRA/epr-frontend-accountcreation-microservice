using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class LimitedPartnershipTypeRequestViewModel
{
    [Required(ErrorMessage = "LimitedPartnershipType.ErrorMessage")]
    public bool? isIndividualPartners { get; set; }
    public bool? isCompanyPartners { get; set; }
}


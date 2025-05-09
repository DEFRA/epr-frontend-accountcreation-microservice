namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class LimitedPartnershipPartnersViewModel
{
    public bool ExpectsIndividualPartners { get; set; }

    public bool ExpectsCompanyPartners { get; set; }

    public List<LimitedPartnershipPersonOrCompanyViewModel>? Partners { get; set; }   
}

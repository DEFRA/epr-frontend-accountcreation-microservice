namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class PartnershipPartnersViewModel
{
    public bool ExpectsIndividualPartners { get; set; }

    public bool ExpectsCompanyPartners { get; set; }

    public List<PartnershipPersonOrCompanyViewModel>? Partners { get; set; }

    public string? DeleteAction { get; set; }
}

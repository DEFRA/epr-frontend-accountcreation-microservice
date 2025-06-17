using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExCheckYourDetailsViewModel
{
    public bool? IsRegisteredAsCharity { get; set; }

    public OrganisationType? OrganisationType { get; set; }

    public ProducerType? ProducerType { get; set; } = Core.Sessions.ProducerType.NotSet;

    public RoleInOrganisation? RoleInOrganisation { get; set; } = Core.Sessions.RoleInOrganisation.NoneOfTheAbove;

    public Nation? Nation { get; set; } = Core.Sessions.Nation.NotSet;

    public string? CompanyName { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public bool? IsTradingNameDifferent { get; set; }

    public string TradingName { get; set; }

    public Address? BusinessAddress { get; set; }

    public bool IsCompaniesHouseFlow { get; set; }
    public string TypeOfProducer => GetCheckYourDetailsKey(ProducerType);
    public string YourRole => GetCheckYourDetailsKey(RoleInOrganisation);
    public string UkNation => GetCheckYourDetailsKey(Nation);
    public List<ReExCompanyTeamMember> reExCompanyTeamMembers { get; set; }
    public bool IsOrganisationAPartnership { get; set; }
    public List<ReExLimitedPartnershipPersonOrCompany>? LimitedPartnershipPartners { get; set; }
    public bool IsLimitedLiabilityPartnership { get; set; }

    public bool IsManualInputFlow { get; set; }

    public bool IsSoleTrader { get; set; }

    private static string GetCheckYourDetailsKey<TEnum>(TEnum? enumValue) where TEnum : struct, Enum
    {
        if (enumValue == null || !Enum.IsDefined(typeof(TEnum), enumValue))
        {
            return string.Empty;
        }

        return $"CheckYourDetails.{enumValue}";
    }
}

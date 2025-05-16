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

    public string TypeOfProducer => ProducerType switch
    {
        Core.Sessions.ProducerType.NonUkOrganisation => "CheckYourDetails.NonUkOrganisation",
        Core.Sessions.ProducerType.Partnership => "CheckYourDetails.Partnership",
        Core.Sessions.ProducerType.SoleTrader => "CheckYourDetails.SoleTrader",
        Core.Sessions.ProducerType.UnincorporatedBody => "CheckYourDetails.UnincorporatedBody",
        Core.Sessions.ProducerType.Other => "CheckYourDetails.Other",
        _ => String.Empty
    };

    public string YourRole => RoleInOrganisation switch
    {
        Core.Sessions.RoleInOrganisation.Director => "CheckYourDetails.Director",
        Core.Sessions.RoleInOrganisation.CompanySecretary => "CheckYourDetails.CompanySecretary",
        Core.Sessions.RoleInOrganisation.Partner => "CheckYourDetails.Partner",
        Core.Sessions.RoleInOrganisation.Member => "CheckYourDetails.Member",
        _ => String.Empty
    };

    public string UkNation => Nation switch
    {
        Core.Sessions.Nation.England => "CheckYourDetails.England",
        Core.Sessions.Nation.Scotland => "CheckYourDetails.Scotland",
        Core.Sessions.Nation.Wales => "CheckYourDetails.Wales",
        Core.Sessions.Nation.NorthernIreland => "CheckYourDetails.NorthernIreland",
        _ => String.Empty
    };

    public string? JobTitle { get; set; }

    public List<ReExCompanyTeamMember> reExCompanyTeamMembers { get; set; }
}

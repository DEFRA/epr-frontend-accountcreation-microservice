using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;

public class ReExUnincorporatedCheckYourDetailsViewModel
{
    public string TradingName { get; set; } = default!;

    public Address? BusinessAddress { get; set; }

    public string? JobTitle { get; set; }

    public IDictionary<Guid, ReExCompanyTeamMember>? TeamMemberDetailsDictionary { get; set; }

    public Nation? Nation { get; set; } = Core.Sessions.Nation.NotSet;
    
    public string UkNation => Nation switch
    {
        Core.Sessions.Nation.England => "ReExCheckYourDetails.England",
        Core.Sessions.Nation.Scotland => "ReExCheckYourDetails.Scotland",
        Core.Sessions.Nation.Wales => "ReExCheckYourDetails.Wales",
        Core.Sessions.Nation.NorthernIreland => "ReExCheckYourDetails.NorthernIreland",
        _ => string.Empty
    };
}
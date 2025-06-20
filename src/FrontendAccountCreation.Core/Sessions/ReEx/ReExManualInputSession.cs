using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Addresses;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

[ExcludeFromCodeCoverage]
public class ReExManualInputSession
{
    public string TradingName { get; set; }

    public string NonUkOrganisationName { get; set; }

    public string NonUkRoleInOrganisation { get; set; }

    /// <summary>
    /// Sets regulator's nation for the non-UK organisation.
    /// </summary>
    public Nation? UkRegulatorNation { get; set; }

    public ProducerType? ProducerType { get; set; }

    public Address? BusinessAddress { get; set; }

    public ReExCompanyTeamMember? TeamMember { get; set; }
}

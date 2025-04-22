using FrontendAccountCreation.Core.Addresses;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class ReExManualInputSession
{
    public string TradingName { get; set; }

    public string RoleInOrganisation { get; set; } = default!;

    public string OrganisationId { get; set; }

    public ProducerType? ProducerType { get; set; }

    public Address? BusinessAddress { get; set; }

    public List<Address?> AddressesForPostcode { get; set; } = [];
}

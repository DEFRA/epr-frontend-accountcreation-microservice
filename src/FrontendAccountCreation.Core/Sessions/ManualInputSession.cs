using FrontendAccountCreation.Core.Addresses;

namespace FrontendAccountCreation.Core.Sessions;

public class ManualInputSession
{
    public string TradingName { get; set; }

    public string RoleInOrganisation { get; set; } = default!;

    public ProducerType? ProducerType { get; set; }

    public Address? BusinessAddress { get; set; }

    public List<Address?> AddressesForPostcode { get; set; } = new();
}

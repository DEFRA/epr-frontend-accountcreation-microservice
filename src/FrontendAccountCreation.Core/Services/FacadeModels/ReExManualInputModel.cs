using FrontendAccountCreation.Core.Sessions;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

/// <summary>
/// Used for non-company journey i.e. SoleTrader, partnership
/// </summary>
[ExcludeFromCodeCoverage]
public class ReExManualInputModel
{
    public string TradingName { get; set; }

    public ProducerType? ProducerType { get; set; }

    public AddressModel? BusinessAddress { get; set; }
}

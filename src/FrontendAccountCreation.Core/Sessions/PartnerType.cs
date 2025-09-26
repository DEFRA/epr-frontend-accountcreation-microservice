using System.ComponentModel;

namespace FrontendAccountCreation.Core.Sessions;

public enum PartnerType
{
    [Description("Individual Partner")]
    IndividualPartner = 1,

    [Description("Corporate Partner")]
    CorporatePartner = 2,
}

using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Sessions.ReEx.Partnership;

[ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
public class ReExLimitedPartnership
{
    public bool HasIndividualPartners { get; set; }

    public bool HasCompanyPartners { get; set; }

    public List<ReExLimitedPartnershipPersonOrCompany>? Partners { get; set; }
}
using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Core.Sessions.ReEx.Partnership;

[ExcludeFromCodeCoverage]
public class ReExLimitedPartnership
{
    public bool HasIndividualPartners { get; set; }

    public bool HasCompanyPartners { get; set; }

    public List<ReExLimitedPartnershipPersonOrCompany>? Partners { get; set; }
}
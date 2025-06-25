using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Core.Sessions.ReEx.Partnership;

[ExcludeFromCodeCoverage]
public class ReExTypesOfPartner
{
    public bool HasIndividualPartners { get; set; }

    public bool HasCompanyPartners { get; set; }

    public List<ReExPersonOrCompanyPartner>? Partners { get; set; }
}
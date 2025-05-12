using FrontendAccountCreation.Core.Sessions.ReEx.Partnership.ApprovedPersons;

namespace FrontendAccountCreation.Core.Sessions.ReEx.Partnership;

public class ReExLimitedPartnership
{
    public ReExLimitedPartnershipSummary? PartnershipSummary { get; set; }
    public List<ReExLimitedPartnershipApprovedPerson>? PartnershipApprovedPersons { get; set; }
}
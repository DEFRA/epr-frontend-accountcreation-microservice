namespace FrontendAccountCreation.Core.Sessions.ReEx.Partnership;

public class ReExPartnership
{
    public bool IsLimitedPartnership { get; set; }

    public ReExLimitedPartnership? LimitedPartnership { get; set; }

    public bool IsLimitedLiabilityPartnership { get; set; }

    public ReExLimitedLiabilityPartnership? LimitedLiabilityPartnership { get; set; }
}

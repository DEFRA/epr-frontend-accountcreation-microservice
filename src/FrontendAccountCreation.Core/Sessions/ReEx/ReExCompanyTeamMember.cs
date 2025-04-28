namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class ReExCompanyTeamMember
{
    public Guid Id { get; set; }

    public bool IsInvited { get; set; }

    public ReExTeamMemberRole? Role { get; set; }
}

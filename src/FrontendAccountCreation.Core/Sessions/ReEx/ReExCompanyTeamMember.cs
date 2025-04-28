namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class ReExCompanyTeamMember
{
    public Guid Id { get; set; }

    public bool IsInvited { get; set; }

    public ReExTeamMemberRole? Role { get; set; }

    public string FullName { get; set; }
    public string TelephoneNumber { get; set; }
    public string Email { get; set; }
}

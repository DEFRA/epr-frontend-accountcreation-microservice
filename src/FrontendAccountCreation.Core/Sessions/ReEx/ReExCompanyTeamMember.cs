namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class ReExCompanyTeamMember
{
    public string FullName { get; set; }

    public string Email  { get; set; }

    public string Telephone { get; set; }

    public bool IsInvited { get; set; }

    public ReExTeamMemberRole? Role { get; set; }
}

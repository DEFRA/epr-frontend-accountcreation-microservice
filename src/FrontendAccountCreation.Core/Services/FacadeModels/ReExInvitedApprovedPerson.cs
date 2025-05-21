namespace FrontendAccountCreation.Core.Services.FacadeModels;

// ReExCompanyTeamMemberModel
public class ReExInvitedApprovedPerson
{
    public Guid? Id { get; set; }
    public string? Role { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TelephoneNumber { get; set; }
    public string Email { get; set; }
}
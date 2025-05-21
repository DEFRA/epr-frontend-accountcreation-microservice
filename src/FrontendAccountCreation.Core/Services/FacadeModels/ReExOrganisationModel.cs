namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class ReExOrganisationModel
{
    /// <summary>
    /// User Role/Job title can be Director, CompanySecretary, Partner or Member
    /// </summary>
    public string? UserRoleInOrganisation { get; set; }

    public ReExCompanyModel Company { get; set; }

    /// <summary>
    /// Approved persons information related to names/emails
    /// </summary>
    public List<ReExInvitedApprovedPerson?> InvitedApprovedPersons { get; set; }
}

using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

[ExcludeFromCodeCoverage]
public class ReExOrganisationModel
{
    /// <summary>
    /// User Role/Job title can be Director, CompanySecretary, Partner or Member
    /// </summary>
    public string? UserRoleInOrganisation { get; set; }

    /// <summary>
    /// User's service role i.e. ApprovedPerson, Basic or Delegated
    /// </summary>
    public string? ServiceRole { get; set; }

    public bool IsApprovedUser { get; set; }

    public ReExCompanyModel Company { get; set; }

    /// <summary>
    /// Approved persons information related to names/emails
    /// </summary>
    public List<ReExInvitedApprovedPerson?> InvitedApprovedPersons { get; set; }
}

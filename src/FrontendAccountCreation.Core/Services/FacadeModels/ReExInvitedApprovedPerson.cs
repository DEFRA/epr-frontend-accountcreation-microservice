using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

[ExcludeFromCodeCoverage]
public class ReExInvitedApprovedPerson
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Approved person Role/Job title can be Director, CompanySecretary
    /// </summary>
    public string? Role { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string TelephoneNumber { get; set; }

    public string Email { get; set; }
}
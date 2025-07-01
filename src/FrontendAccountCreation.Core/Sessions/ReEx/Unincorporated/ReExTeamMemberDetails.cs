using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Sessions.ReEx.Unincorporated;

[ExcludeFromCodeCoverage]
public class ReExTeamMemberDetails
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string Telephone { get; set; }
}

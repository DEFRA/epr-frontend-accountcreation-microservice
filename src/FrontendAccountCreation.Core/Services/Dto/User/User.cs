namespace FrontendAccountCreation.Core.Services.Dto.User;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class User
{
    public Guid Id { get; set; }

    public string EnrolmentStatus { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}
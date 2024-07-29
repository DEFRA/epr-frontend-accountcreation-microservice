using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;

[ExcludeFromCodeCoverage]
public record OrganisationData
{
    public DateTime? DateOfCreation { get; init; }

    public string? Status { get; init; }

    public string? Type { get; init; }
}

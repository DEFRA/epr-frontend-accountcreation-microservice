using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;

[ExcludeFromCodeCoverage]
public record Country
{
    public string? Name { get; init; }

    public string? Iso { get; init; }
}

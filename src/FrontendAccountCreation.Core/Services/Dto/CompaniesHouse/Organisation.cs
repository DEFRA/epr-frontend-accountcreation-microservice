using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;

[ExcludeFromCodeCoverage]
public record Organisation
{
    public string? Name { get; init; }

    public string? RegistrationNumber { get; init; }

    public RegisteredOfficeAddress? RegisteredOffice { get; init; }

    public OrganisationData? OrganisationData { get; init; }
}

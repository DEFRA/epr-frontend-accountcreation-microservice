namespace FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;

public record Organisation
{
    public string? Name { get; init; }

    public string? RegistrationNumber { get; init; }

    public RegisteredOfficeAddress? RegisteredOffice { get; init; }

    public OrganisationData? OrganisationData { get; init; }
}

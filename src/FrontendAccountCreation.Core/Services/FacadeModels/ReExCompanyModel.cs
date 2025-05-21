using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class ReExCompanyModel
{
    public string? OrganisationId { get; set; }

    public string? OrganisationType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    public string CompanyName { get; set; }

    public AddressModel? CompanyRegisteredAddress { get; set; }

    public bool ValidatedWithCompaniesHouse { get; set; }

    public Nation? Nation { get; set; }
}

using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

[ExcludeFromCodeCoverage]
public class ReExCompanyModel
{
    public string? OrganisationId { get; set; }

    public OrganisationType? OrganisationType { get; set; }

    //Type of producers for comapany house can be LP or LLP
    public string? ProducerType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    public string CompanyName { get; set; }

    public AddressModel? CompanyRegisteredAddress { get; set; }

    public bool ValidatedWithCompaniesHouse { get; set; }

    public Nation? Nation { get; set; }

    public bool IsComplianceScheme { get; set; }
}

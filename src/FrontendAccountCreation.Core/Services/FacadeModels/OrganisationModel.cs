using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class OrganisationModel
{
    public OrganisationType? OrganisationType { get; set; }

    public ProducerType? ProducerType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    public AddressModel Address { get; set; }

    public bool ValidatedWithCompaniesHouse { get; set; }
    
    public bool IsComplianceScheme { get; set; }

    public Nation? Nation { get; set; }
    
    public string OrganisationId { get; set; }
}

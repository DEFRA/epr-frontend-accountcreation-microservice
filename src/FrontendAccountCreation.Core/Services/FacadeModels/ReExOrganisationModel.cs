using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class ReExOrganisationModel
{
    public OrganisationType? OrganisationType { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    [MaxLength(100)]
    public string CompanyName { get; set; }

    public AddressModel? CompanyAddress { get; set; }

    public bool ValidatedWithCompaniesHouse { get; set; }

    public Nation? Nation { get; set; }

    public string OrganisationId { get; set; }
}

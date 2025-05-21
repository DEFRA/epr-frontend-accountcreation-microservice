using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class ReExOrganisationModel
{
    public string? OrganisationId { get; set; }

    public string? OrganisationType { get; set; }

    /// <summary>
    /// Role can be Director, CompanySecretary, Partner or Member
    /// </summary>
    public string? RoleInOrganisation { get; set; }

    public string? CompaniesHouseNumber { get; set; }

    [MaxLength(100)]
    public string CompanyName { get; set; }

    public AddressModel? CompanyAddress { get; set; }

    public bool ValidatedWithCompaniesHouse { get; set; }

    public Nation? Nation { get; set; }

    /// <summary>
    /// Approved persons information related to names/emails
    /// </summary>
    public List<ReExInvitedApprovedPerson?> InvitedApprovedPersons { get; set; }
}

namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class ApprovedPersonOrganisationModel
{
    public string? SubBuildingName { get; set; }

    public string? BuildingName { get; set; }

    public string? BuildingNumber { get; set; }

    public string? Street { get; set; }

    public string? Town { get; set; }

    public string? County { get; set; }

    public string? Postcode { get; set; }

    public string? Locality { get; set; }

    public string? DependentLocality { get; set; }

    public string? Country { get; set; }

    public bool? IsUkAddress { get; set; }
    public string? OrganisationName { get; set; }
    
    public string? ApprovedUserEmail { get; set; }
}
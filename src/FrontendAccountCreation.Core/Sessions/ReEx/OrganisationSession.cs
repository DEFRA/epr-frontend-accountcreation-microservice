namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class OrganisationSession
{
    public List<string> Journey { get; set; } = new();

    public bool IsTheOrganisationCharity { get; set; }

    public OrganisationType? OrganisationType { get; set; }

    public Nation? UkNation { get; set; }

    /// <summary>
    /// ReEx Companies-House session
    /// </summary>
    public ReExCompaniesHouseSession? CompaniesHouseSession { get; set; }

    /// <summary>
    /// ReEx Manual input session
    /// </summary>
    public ReExManualInputSession? ManualInputSession { get; set; }

    public Contact? Contact { get; set; } = new();

    public string? DeclarationFullName { get; set; }

    public DateTime DeclarationTimestamp { get; set; }

    public string? InviteToken { get; set; }

    public bool IsCompaniesHouseFlow => OrganisationType == Sessions.OrganisationType.CompaniesHouseCompany;

    public bool IsManualInputFlow => OrganisationType == Sessions.OrganisationType.NonCompaniesHouseCompany;

    public bool IsUserChangingDetails { get; set; }

    public bool IsApprovedUser { get; set; }

    public string OrganisationId { get; set; }
}

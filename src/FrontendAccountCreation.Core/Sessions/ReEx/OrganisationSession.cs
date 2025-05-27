namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class OrganisationSession
{
    public List<string> Journey { get; set; } = [];

    public bool? IsTheOrganisationCharity { get; set; }

    public OrganisationType? OrganisationType { get; set; }

    public Nation? UkNation { get; set; }

    /// <summary>
    /// ReEx Companies-House session
    /// </summary>
    public ReExCompaniesHouseSession? ReExCompaniesHouseSession { get; set; }

    /// <summary>
    /// ReEx Manual input session
    /// </summary>
    public ReExManualInputSession? ReExManualInputSession { get; set; }

    public Contact? Contact { get; set; } = new();

    public string? DeclarationFullName { get; set; }

    public DateTime DeclarationTimestamp { get; set; }

    public bool IsCompaniesHouseFlow => OrganisationType == Sessions.OrganisationType.CompaniesHouseCompany;

    public bool IsUserChangingDetails { get; set; }

    public bool IsApprovedUser { get; set; }

    public bool? IsTradingNameDifferent { get; set; }

    public bool? IsOrganisationAPartnership { get; set; }

    /// <summary>
    /// User's service role i.e. ApprovedPerson, Basic or Delegated
    /// </summary>
    public string? ServiceRole { get; set; }
}

namespace FrontendAccountCreation.Core.Sessions;

public class AccountCreationSession
{
    public List<string> Journey { get; set; } = new();

    public bool IsTheOrganisationCharity { get; set; }

    public OrganisationType? OrganisationType { get; set; }

    public Nation? UkNation { get; set; }

    public CompaniesHouseSession? CompaniesHouseSession { get; set; }

    public ManualInputSession? ManualInputSession { get; set; }

    public Contact? Contact { get; set; } = new();
    
    public string? InviteToken { get; set; }

    public bool IsCompaniesHouseFlow => OrganisationType == Sessions.OrganisationType.CompaniesHouseCompany;

    public bool IsManualInputFlow => OrganisationType == Sessions.OrganisationType.NonCompaniesHouseCompany;

    public bool IsUserChangingDetails { get; set; }
    
    public bool IsApprovedUser { get; set; }
}

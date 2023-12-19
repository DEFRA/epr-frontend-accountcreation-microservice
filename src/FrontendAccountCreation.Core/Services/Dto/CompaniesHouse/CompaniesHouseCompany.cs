namespace FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;

public record CompaniesHouseCompany
{
    public Organisation? Organisation { get; init; }

    public bool AccountExists { get; set; }
    public DateTimeOffset? AccountCreatedOn { get; set; }

}

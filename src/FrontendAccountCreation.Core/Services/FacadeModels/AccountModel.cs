namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class AccountModel
{
    public PersonModel Person { get; set; }

    public OrganisationModel Organisation { get; set; }

    public ConnectionModel Connection { get; set; }

    public string? DeclarationFullName { get; set; }

    public DateTime? DeclarationTimeStamp { get; set; }

}
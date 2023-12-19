namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class AccountAlreadyExistsViewModel
{
    public DateTime DateCreated { get; set; }

    public string DisplayDateCreated { get { return DateCreated.ToString("d MMMM yyyy"); } }
}

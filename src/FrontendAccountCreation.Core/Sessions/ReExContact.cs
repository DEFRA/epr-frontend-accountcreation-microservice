namespace FrontendAccountCreation.Core.Sessions;

public class ReExContact
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string TelephoneNumber { get; set; }

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}
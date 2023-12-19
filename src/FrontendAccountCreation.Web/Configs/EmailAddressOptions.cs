namespace FrontendAccountCreation.Web.Configs;

public class EmailAddressOptions
{
    public const string ConfigSection = "EmailAddresses";

    public string DataProtection { get; set; }

    public string DefraGroupProtectionOfficer { get; set; }

    public string DefraHelpline { get; set; }
}

using System.Globalization;

namespace FrontendAccountCreation.Web.ViewModels.Shared;

public class LanguageSwitcherModel
{
    public CultureInfo? CurrentCulture { get; set; }

    public List<CultureInfo>? SupportedCultures { get; set; }

    public string? ReturnUrl { get; set; }
    
    public bool ShowLanguageSwitcher { get; set; }
}
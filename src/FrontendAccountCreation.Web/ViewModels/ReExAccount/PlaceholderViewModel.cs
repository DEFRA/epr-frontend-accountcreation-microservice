using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public record PlaceholderViewModel
{
    public string PageTitle { get; set; }
    public bool Interstitial { get; set; }
}
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class ReExPersonOrCompanyPartner
{
    public Guid Id { get; set; }

    /// <summary>
    /// Person or company name. Person name if IsPerson = true, else Company name.
    /// </summary>
    public string Name { get; set; }

    public bool IsPerson { get; set; }
}
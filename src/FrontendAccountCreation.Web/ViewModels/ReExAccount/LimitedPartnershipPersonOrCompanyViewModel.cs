using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

/// <summary>
/// Can hold person name or company names, depending on IsPerson is true or false
/// </summary>
public class LimitedPartnershipPersonOrCompanyViewModel
{
    [Required]
    public Guid Id { get; set; }

    public string? PersonName { get; set; }

    public string? CompanyName { get; set; }

    public bool IsPerson => !string.IsNullOrWhiteSpace(PersonName);

    public bool IsCompany => !string.IsNullOrWhiteSpace(CompanyName);

    [RegularExpression("True|true", ErrorMessage = "Must be a company or a person but not both")]
    public bool IsPersonOrCompanyButNotBoth => ((IsPerson && !IsCompany) || (IsCompany && !IsPerson));
}
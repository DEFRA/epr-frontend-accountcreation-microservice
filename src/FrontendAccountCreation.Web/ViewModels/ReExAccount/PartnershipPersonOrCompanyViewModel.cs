using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

/// <summary>
/// Can hold person name or company names, depending on IsPerson is true or false
/// </summary>
public class PartnershipPersonOrCompanyViewModel
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(100, ErrorMessage = "Partners.NameLengthError")]
    public string? CompanyName { get; set; }

    [MaxLength(100, ErrorMessage = "Partners.NameLengthError")]
    public string? PersonName { get; set; }

    public bool IsPerson => !string.IsNullOrWhiteSpace(PersonName);

    public bool IsCompany => !string.IsNullOrWhiteSpace(CompanyName);

    [RegularExpression("True|true")]
    public bool IsPersonOrCompanyButNotBoth => ((IsPerson && !IsCompany) || (IsCompany && !IsPerson));

    public static implicit operator PartnershipPersonOrCompanyViewModel(ReExPersonOrCompanyPartner partner)
    {
        return new PartnershipPersonOrCompanyViewModel
        {
            Id = partner.Id,
            PersonName = partner.IsPerson? partner.Name : null,
            CompanyName = !partner.IsPerson ? partner.Name: null
        };
    }
}
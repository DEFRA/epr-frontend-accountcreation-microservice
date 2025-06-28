using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class BusinessAddressOverseasViewModel
{
    [Required(ErrorMessage = "AddressOverseas.CountryNameError")]
    [MaxLength(54, ErrorMessage = "AddressOverseas.CountryLengthError")]
    [RegularExpression(@"^[a-zA-ZÀ-ÖØ-öø-ÿ '\-]+$", ErrorMessage = "AddressOverseas.CountryInvalidError")]
    public string? Country { get; set; }

    [Required(ErrorMessage = "AddressOverseas.AddressLine1Error")]
    [MaxLength(100, ErrorMessage = "AddressOverseas.AddressLine1LengthError")]
    public string? AddressLine1 { get; set; }
        
    [MaxLength(100, ErrorMessage = "AddressOverseas.AddressLine2LengthError")]
    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "AddressOverseas.TownOrCityError")]
    [MaxLength(70, ErrorMessage = "AddressOverseas.TownOrCityLengthError")]
    public string? TownOrCity { get; set; }

    [MaxLength(50, ErrorMessage = "AddressOverseas.StateProvinceRegionLengthError")]
    public string? StateProvinceRegion { get; set; }

    [MaxLength(15, ErrorMessage = "AddressOverseas.PostcodeLengthError")]
    public string? Postcode { get; set; }
}
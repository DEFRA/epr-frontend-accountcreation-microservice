using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class BusinessAddressOverseasViewModel
{
    [Required(ErrorMessage = "BusinessAddressOverseas.CountryNameError")]
    [MaxLength(100, ErrorMessage = "BusinessAddressOverseas.CountryLengthError")]
    public string? Country { get; set; }

    [Required(ErrorMessage = "BusinessAddressOverseas.AddressLine1NameError")]
    [MaxLength(100, ErrorMessage = "BusinessAddressOverseas.AddressLine1LengthError")]
    public string? AddressLine1 { get; set; }

    [Required(ErrorMessage = "BusinessAddressOverseas.AddressLine2Error")]
    [MaxLength(100, ErrorMessage = "BusinessAddressOverseas.AddressLine2LengthError")]
    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "BusinessAddressOverseas.TownOrCityError")]
    [MaxLength(70, ErrorMessage = "BusinessAddressOverseas.TownOrCityLengthError")]
    public string? TownOrCity { get; set; }

    [MaxLength(50, ErrorMessage = "BusinessAddressOverseas.StateProvinceRegionLengthError")]
    public string? StateProvinceRegion { get; set; }

    [Required(ErrorMessage = "BusinessAddressOverseas.PostcodeError")]
    [RegularExpression("^([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$",
        ErrorMessage = "BusinessAddressOverseas.PostcodeInvalidError")]
    public string? Postcode { get; set; }
}
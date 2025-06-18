using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class BusinessAddressOverseasViewModel
{
    //todo: reconcile with the errors in the story, e.g. invalid country name
    
    [Required(ErrorMessage = "AddressOverseas.CountryNameError")]
    [MaxLength(100, ErrorMessage = "AddressOverseas.CountryLengthError")]
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

    [Required(ErrorMessage = "AddressOverseas.PostcodeError")]
    [RegularExpression("^([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$",
        ErrorMessage = "AddressOverseas.PostcodeInvalidError")]
    public string? Postcode { get; set; }
}
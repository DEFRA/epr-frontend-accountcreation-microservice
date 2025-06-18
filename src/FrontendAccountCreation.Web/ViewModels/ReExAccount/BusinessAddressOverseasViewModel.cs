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
    public string? Postcode { get; set; }
}
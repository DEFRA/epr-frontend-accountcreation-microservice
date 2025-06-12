using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExBusinessAddressViewModel
{
    //todo: add extra error messages to story
    [Required(ErrorMessage = "BusinessAddress.BuildingNumberError")]
    [MaxLength(50, ErrorMessage = "BusinessAddress.BuildingNumberLengthError")]
    public string? BuildingNumber { get; set; }

    [Required(ErrorMessage = "BusinessAddress.BuildingNameError")]
    [MaxLength(100, ErrorMessage = "BusinessAddress.BuildingNameLengthError")]
    public string? BuildingName { get; set; }

    [Required(ErrorMessage = "BusinessAddress.StreetNameError")]
    [MaxLength(100, ErrorMessage = "BusinessAddress.StreetNameLengthError")]
    public string? Street { get; set; }

    [Required(ErrorMessage = "BusinessAddress.TownError")]
    [MaxLength(70, ErrorMessage = "BusinessAddress.TownLengthError")]
    public string? Town { get; set; }

    [MaxLength(50, ErrorMessage = "BusinessAddress.CountyLengthError")]
    public string? County { get; set; }

    [Required(ErrorMessage = "BusinessAddress.PostcodeError")]
    [RegularExpression("^([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$",
        ErrorMessage = "BusinessAddress.PostcodeInvalidError")]
    public string? Postcode { get; set; }
}
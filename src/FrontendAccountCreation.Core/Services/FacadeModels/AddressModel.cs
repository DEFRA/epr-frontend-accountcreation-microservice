using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class AddressModel
{
    [MaxLength(100)]
    public string? SubBuildingName { get; set; }

    [MaxLength(100)]
    public string? BuildingName { get; set; }

    [MaxLength(50)]
    public string? BuildingNumber { get; set; }

    [MaxLength(100)]
    public string? Street { get; set; }

    [MaxLength(100)]
    public string? Locality { get; set; }

    [MaxLength(100)]
    public string? DependentLocality { get; set; }

    [MaxLength(70)]
    public string? Town { get; set; } = default!;

    [MaxLength(50)]
    public string? County { get; set; } = default!;

    [MaxLength(15)]
    public string? Postcode { get; set; } = default!;

    [MaxLength(54)]
    public string? Country { get; set; } = default!;
}
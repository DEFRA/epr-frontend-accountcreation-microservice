using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

[ExcludeFromCodeCoverage]
public class ReExPersonModel
{
    [MaxLength(50)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string LastName { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string ContactEmail { get; set; }

    [MaxLength(50)]
    public string TelephoneNumber { get; set; }
}
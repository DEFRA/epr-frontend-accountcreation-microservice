using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

public class PersonModel
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
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Services.FacadeModels;

[ExcludeFromCodeCoverage(Justification = "Model no business logic")]
public class ReExPartnerModel
{
    public string Name { get; set; }

    public string PartnerRole { get; set; }
}

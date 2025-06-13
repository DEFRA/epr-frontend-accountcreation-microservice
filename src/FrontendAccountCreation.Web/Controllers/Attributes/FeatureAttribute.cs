namespace FrontendAccountCreation.Web.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FeatureAttribute(string? requiredFeature = null) : Attribute
{
    public string? RequiredFeature => requiredFeature;
}

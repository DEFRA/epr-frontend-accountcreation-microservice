namespace FrontendAccountCreation.Web.Controllers.Attributes;

//todo: check works on method too
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FeatureAttribute(string? requiredFeature = null) : Attribute
{
    public string? RequiredFeature => requiredFeature;
}

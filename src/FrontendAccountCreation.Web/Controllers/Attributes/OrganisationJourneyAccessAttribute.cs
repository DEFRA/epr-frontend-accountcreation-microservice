namespace FrontendAccountCreation.Web.Controllers.Attributes;

// we might have to expand the feature handling to accept a collection (or collection for and/or handling), but we don't need it yet
// we don't strictly need to explicitly specify the requiredFeature, but this moves us closer to generic components to handle the journey
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OrganisationJourneyAccessAttribute(string pagePath, string? requiredFeature = null) : Attribute
{
    public string PagePath => pagePath;
    public string? RequiredFeature => requiredFeature;
}

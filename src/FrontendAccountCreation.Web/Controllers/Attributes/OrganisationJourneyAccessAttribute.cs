namespace FrontendAccountCreation.Web.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class OrganisationJourneyAccessAttribute : Attribute
{
    public OrganisationJourneyAccessAttribute(string pagePath)
    {
        PagePath = pagePath;
    }

    public string PagePath { get; }
}

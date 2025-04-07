namespace FrontendAccountCreation.Web.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ReprocessorExporterJourneyAccessAttribute : Attribute
{
    public ReprocessorExporterJourneyAccessAttribute(string pagePath)
    {
        PagePath = pagePath;
    }

    public string PagePath { get; }
}

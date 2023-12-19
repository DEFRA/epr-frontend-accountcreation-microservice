namespace FrontendAccountCreation.Web.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class JourneyAccessAttribute : Attribute
{
    public JourneyAccessAttribute(string pagePath)
    {
        PagePath = pagePath;
    }

    public string PagePath { get; }
}

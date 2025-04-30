using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Configs;

[ExcludeFromCodeCoverage]
public class ServiceKeysOptions
{
    public const string ConfigSection = "ServiceKeys";

    public string ReprocessorExporter { get; set; }
}
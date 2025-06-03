using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Web.Configs;

[ExcludeFromCodeCoverage]
public class MultipleOptions(
    IOptions<ExternalUrlsOptions> urlOptions, 
    IOptions<ServiceKeysOptions> serviceKeyOptions) : IMultipleOptions
{
    private readonly ExternalUrlsOptions _urlOptions = urlOptions.Value;
    private readonly ServiceKeysOptions _serviceKeyOptions = serviceKeyOptions.Value;

    public ExternalUrlsOptions UrlOptions { get { return _urlOptions; } }

    public ServiceKeysOptions ServiceKeysOptions { get { return _serviceKeyOptions; } }
}

namespace FrontendAccountCreation.Web.Configs
{
    public interface IMultipleOptions
    {
        ExternalUrlsOptions UrlOptions { get; }
        ServiceKeysOptions ServiceKeysOptions { get; }
    }
}
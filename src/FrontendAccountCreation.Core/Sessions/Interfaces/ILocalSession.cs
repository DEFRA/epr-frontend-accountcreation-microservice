namespace FrontendAccountCreation.Core.Sessions.Interfaces;

public interface ILocalSession
{
    public List<string> Journey { get; set; }

    public bool IsUserChangingDetails { get; set; }
}

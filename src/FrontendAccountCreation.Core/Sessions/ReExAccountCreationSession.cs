using FrontendAccountCreation.Core.Sessions.Interfaces;

namespace FrontendAccountCreation.Core.Sessions;

public class ReExAccountCreationSession : ILocalSession
{
    public ReExContact? Contact { get; set; } = new();
    public bool IsUserChangingDetails { get; set; }
    public List<string> Journey { get; set; } = new();
}

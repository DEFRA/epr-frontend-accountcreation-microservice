namespace FrontendAccountCreation.Core.Sessions;

public class ReExAccountCreationSession
{
    public List<string> Journey { get; set; } = new();

    public ReExContact? Contact { get; set; } = new();
}

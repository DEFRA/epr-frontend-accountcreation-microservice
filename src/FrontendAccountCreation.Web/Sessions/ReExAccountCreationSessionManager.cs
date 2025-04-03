using FrontendAccountCreation.Core.Sessions;
using System.Text.Json;

namespace FrontendAccountCreation.Web.Sessions;

//todo: write a generic session manager and repplace this and AccountCreationSessionManager with it
public class ReExAccountCreationSessionManager : ISessionManager<ReExAccountCreationSession>
{
    private readonly string _sessionKey = nameof(ReExAccountCreationSession);

    private ReExAccountCreationSession? _sessionValue;

    public async Task<ReExAccountCreationSession?> GetSessionAsync(ISession session)
    {
        if (_sessionValue != null)
        {
            return _sessionValue;
        }

        await session.LoadAsync();

        var sessionString = session.GetString(_sessionKey);

        if (sessionString != null)
        {
            _sessionValue = JsonSerializer.Deserialize<ReExAccountCreationSession>(sessionString);
        }

        return _sessionValue;
    }

    public void RemoveSession(ISession session)
    {
        session.Remove(_sessionKey);

        _sessionValue = null;
    }

    public async Task SaveSessionAsync(ISession session, ReExAccountCreationSession sessionValue)
    {
        await session.LoadAsync();

        session.SetString(_sessionKey, JsonSerializer.Serialize(sessionValue));

        _sessionValue = sessionValue;
    }

    public async Task UpdateSessionAsync(ISession session, Action<ReExAccountCreationSession> updateFunc)
    {
        var sesionValue = await GetSessionAsync(session) ?? new ReExAccountCreationSession();

        updateFunc(sesionValue);

        await SaveSessionAsync(session, sesionValue);
    }
}

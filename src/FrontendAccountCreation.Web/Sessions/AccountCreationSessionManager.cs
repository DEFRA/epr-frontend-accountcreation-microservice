using FrontendAccountCreation.Core.Sessions;

using System.Text.Json;

namespace FrontendAccountCreation.Web.Sessions;

public class AccountCreationSessionManager : ISessionManager<AccountCreationSession>
{
    private readonly string _sessionKey = nameof(AccountCreationSession);

    private AccountCreationSession? _sessionValue;

    public async Task<AccountCreationSession?> GetSessionAsync(ISession session)
    {
        if (_sessionValue != null)
        {
            return _sessionValue;
        }

        await session.LoadAsync();

        var sessionString = session.GetString(_sessionKey);

        if (sessionString != null)
        {
            _sessionValue = JsonSerializer.Deserialize<AccountCreationSession>(sessionString);
        }

        return _sessionValue;
    }

    public void RemoveSession(ISession session)
    {
        session.Remove(_sessionKey);

        _sessionValue = null;
    }

    public async Task SaveSessionAsync(ISession session, AccountCreationSession sessionValue)
    {
        await session.LoadAsync();

        session.SetString(_sessionKey, JsonSerializer.Serialize(sessionValue));

        _sessionValue = sessionValue;
    }

    public async Task UpdateSessionAsync(ISession session, Action<AccountCreationSession> updateFunc)
    {
        var sesionValue = await GetSessionAsync(session) ?? new AccountCreationSession();

        updateFunc(sesionValue);

        await SaveSessionAsync(session, sesionValue);
    }
}

namespace FrontendAccountCreation.Web.UnitTests.Sessions;

using FluentAssertions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

[TestClass]
public class SessionRequestCultureProviderTests
{
    [TestMethod]
    public async Task DetermineProviderCultureResult_ReturnsEnglish_WhenSessionLanguageKeyIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();

        var provider = new SessionRequestCultureProvider();

        // Act
        var result = await provider.DetermineProviderCultureResult(httpContext);

        // Assert
        result.Should().NotBeNull();
        result.Cultures[0].Value.Should().Be(Language.English);
    }

    [TestMethod]
    public async Task DetermineProviderCultureResult_ReturnsSessionLanguage_WhenSessionLanguageKeyIsNotNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.Session.SetString(Language.SessionLanguageKey, "fr");

        var provider = new SessionRequestCultureProvider();

        // Act
        var result = await provider.DetermineProviderCultureResult(httpContext);

        // Assert
        result.Should().NotBeNull();
        result.Cultures[0].Value.Should().Be("fr");
    }

    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);

        public void SetString(string key, string value)
        {
            _sessionStorage[key] = System.Text.Encoding.UTF8.GetBytes(value);
        }

        public string GetString(string key)
        {
            return _sessionStorage.TryGetValue(key, out var data) ? System.Text.Encoding.UTF8.GetString(data) : null;
        }
    }
}

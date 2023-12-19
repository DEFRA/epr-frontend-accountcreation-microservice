using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using System.Text.Json;

namespace FrontendAccountCreation.Web.UnitTests.Sessions;

[TestClass]
public class SessionManagerTests
{
    private readonly AccountCreationSession _testSession = new() { IsTheOrganisationCharity = true};
    private readonly string _sessionKey = nameof(AccountCreationSession);

    private string _serializedTestSession = null!;
    private byte[] _sessionBytes = null!;

    private Mock<ISession> _sessionMock = null!;
    private ISessionManager<AccountCreationSession> _sessionManager = null!;


    [TestInitialize]
    public void Setup()
    {
        _serializedTestSession = JsonSerializer.Serialize(_testSession);
        _sessionBytes = Encoding.UTF8.GetBytes(_serializedTestSession);

        _sessionMock = new Mock<ISession>();
        _sessionManager = new AccountCreationSessionManager();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.TryGetValue(_sessionKey, out _sessionBytes)).Returns(true);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once());

        session!.IsTheOrganisationCharity.Should().Be(_testSession.IsTheOrganisationCharity);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenGetSessionAsyncCalled_ThenSessionReturnedFromMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        var session = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        // Assert
        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.TryGetValue(_sessionKey, out It.Ref<byte[]?>.IsAny), Times.Never);

        session!.IsTheOrganisationCharity.Should().Be(_testSession.IsTheOrganisationCharity);
    }

    [TestMethod]
    public async Task GivenNewSession_WhenSaveSessionAsyncCalled_ThenSessionSavedInStoreAndMemory()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));

        // Act
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Once);

        savedSession.Should().NotBeNull();
        savedSession!.IsTheOrganisationCharity.Should().Be(_testSession.IsTheOrganisationCharity);
    }

    [TestMethod]
    public async Task GivenSessionKey_WhenRemoveSessionCalled_ThenSessionRemovedFromMemoryAndSessionStore()
    {
        // Arrange
        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));

        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        _sessionManager.RemoveSession(_sessionMock.Object);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.Remove(_sessionKey), Times.Once);

        savedSession.Should().BeNull();
    }

    [TestMethod]
    public async Task GivenNoSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Arrange
        const bool isTheOrganisationCharity = true;

        // Act
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.IsTheOrganisationCharity = isTheOrganisationCharity);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Once);

        savedSession.Should().NotBeNull();
        savedSession!.IsTheOrganisationCharity.Should().Be(isTheOrganisationCharity);
    }

    [TestMethod]
    public async Task GivenSessionInMemory_WhenUpdateSessionAsyncCalled_ThenSessionHasBeenUpdatedInMemoryAndStore()
    {
        // Arrange
        const bool isTheOrganisationCharity = true;

        _sessionMock.Setup(x => x.Set(_sessionKey, It.IsAny<byte[]>()));
        await _sessionManager.SaveSessionAsync(_sessionMock.Object, _testSession);

        // Act
        await _sessionManager.UpdateSessionAsync(_sessionMock.Object, (x) => x.IsTheOrganisationCharity = isTheOrganisationCharity);

        // Assert
        var savedSession = await _sessionManager.GetSessionAsync(_sessionMock.Object);

        _sessionMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionMock.Verify(x => x.Set(_sessionKey, It.IsAny<byte[]>()), Times.Exactly(2));

        savedSession.Should().NotBeNull();
        savedSession!.IsTheOrganisationCharity.Should().Be(isTheOrganisationCharity);
    }
}

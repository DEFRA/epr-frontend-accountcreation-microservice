using System.Threading.Tasks;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class YouAreApprovedPersonSoleTraderTests : ApprovedPersonTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_YouAreApprovedPerson_Returns_View()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession {  ProducerType = Core.Sessions.ProducerType.SoleTrader }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.YouAreApprovedPersonSoleTrader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().BeNull();
    }

    [TestMethod]
    public async Task Post_YouAreApprovedPersonSoleTrader_Redirects_To_Desired_View()
    {
        // Arrange
        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession { ProducerType = Core.Sessions.ProducerType.SoleTrader, TradingName = "Test Sole trader", BusinessAddress = new Core.Addresses.Address() }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.SoleTraderContinue();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));
    }
}

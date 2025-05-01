using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using global::FrontendAccountCreation.Web.Constants;
using global::FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class YouAreApprovedPersonTests : OrganisationTestBase
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

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task ContinueCalls_ApprovedConfirmationContinue_And_Redirects_ToDesired_View()
    {
        // Arrange
        var orgSessionMock = new OrganisationSession
        {
            Journey = [PagePath.ApprovedPersonContinue]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgSessionMock)
            .Verifiable();

        // Act
        var result = _systemUnderTest.ApprovedConfirmationContinue();
        var okResult = (Microsoft.AspNetCore.Mvc.StatusCodeResult)result.Result;
        
        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskStatus.RanToCompletion);
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    // TO DO following & modify - once Tungsten has merged
    [TestMethod]
    public async Task InviteLink_Calls_InviteOtherApprovedPerson_And_Redirects_ToDesired_View()
    {
        // Arrange
        var orgSessionMock = new OrganisationSession
        {
            Journey = [PagePath.AddApprovedPerson]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgSessionMock)
            .Verifiable();

        // Act
        var result = _systemUnderTest.InviteOtherApprovedPerson();

        // Assert
        result.Should().NotBeNull();
    }
}

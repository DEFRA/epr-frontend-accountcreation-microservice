using System.Net;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.Errors;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class DeclarationTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_Declaration_Returns_View()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.Declaration();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task ContinueCalls_DeclarationContinue_And_Redirects_ToDesired_View()
    {
        // Arrange
        var orgSessionMock = new OrganisationSession
        {
            Journey = [PagePath.Declaration, PagePath.DeclarationContinue]
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgSessionMock)
            .Verifiable();

        // Act
        var result = _systemUnderTest.DeclarationContinue();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskStatus.RanToCompletion);
    }
}

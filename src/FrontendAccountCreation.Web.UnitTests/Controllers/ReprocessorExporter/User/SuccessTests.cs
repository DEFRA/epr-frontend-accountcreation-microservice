using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.User;

[TestClass]
public class SuccessTests : UserTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Success_HappyPath_SuccessPageReturned()
    {
        //Arrange
        var session = CreateSession();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        //Act
        var result = await _systemUnderTest.Success();

        //Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task Success_HappyPath_ViewModelContainsUsersFullName()
    {
        //Arrange
        var session = CreateSession();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        //Act
        var result = await _systemUnderTest.Success();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<SuccessViewModel>();

        var viewModel = (SuccessViewModel?)viewResult.Model;
        viewModel.UserName.Should().Be("Chris Stapleton");
    }

    [TestMethod]
    public async Task Success_HappyPath_BackLinkShouldBeTelephonePage()
    {
        //Arrange
        var session = CreateSession();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        //Act
        var result = await _systemUnderTest.Success();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.TelephoneNumber);
    }

    //todo:
    // PostReprocessorExporterAccountAsync called
    // session removed

    private static ReExAccountCreationSession CreateSession()
    {
        return new ReExAccountCreationSession
        {
            Journey = [PagePath.FullName, PagePath.TelephoneNumber, PagePath.Success],
            Contact = new ReExContact { FirstName = "Chris", LastName = "Stapleton", TelephoneNumber = "01234567890" }
        };
    }
}

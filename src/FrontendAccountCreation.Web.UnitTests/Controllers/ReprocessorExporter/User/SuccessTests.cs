﻿using FluentAssertions;
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
    public async Task Success_HappyPath_ThereIsNoBackLink()
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

        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeFalse();
    }

    [TestMethod]
    public async Task Success_HappyPath_SessionRemoved()
    {
        //Arrange
        var session = CreateSession();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        //Act
        await _systemUnderTest.Success();

        //Assert
        _sessionManagerMock.Verify(sm => sm.RemoveSession(It.IsAny<ISession>()), Times.Once);
    }

    private static ReExAccountCreationSession CreateSession()
    {
        return new ReExAccountCreationSession
        {
            Journey = [PagePath.FullName, PagePath.TelephoneNumber, PagePath.Success],
            Contact = new ReExContact { FirstName = "Chris", LastName = "Stapleton", TelephoneNumber = "01234567890" }
        };
    }
}

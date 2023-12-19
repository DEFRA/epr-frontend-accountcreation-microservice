using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class CannotCreateAccountTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RoleInOrganisation, PagePath.CannotCreateAccount },
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenCannotCreateAccountCalled_ThenCannotCreateAccountPageReturned_WithRoleInOrganisationAsTheBackLink()
    {
        //Act
        var result = await _systemUnderTest.CannotCreateAccount();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeNull();

        AssertBackLink(viewResult, PagePath.RoleInOrganisation);
    }
}

using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class NotAffectedTests : AccountCreationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task NotAffect_IsCharity_NotAffectedPageReturned()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
                { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.NotAffected },
            OrganisationType = OrganisationType.NonCompaniesHouseCompany,
            IsTheOrganisationCharity = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.NotAffected();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.RegisteredWithCompaniesHouse);
    }
}

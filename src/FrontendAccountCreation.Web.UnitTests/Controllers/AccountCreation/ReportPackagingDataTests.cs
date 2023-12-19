using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class ReportPackagingDataTests: AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public void ReportPackaginData_ReturnsView()
    {
        //Act
        var result = _systemUnderTest.ReportPackagingData();
        
        //Arrange
        result.Should().BeOfType<ViewResult>();
    }

}
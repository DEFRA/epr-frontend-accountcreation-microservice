using FluentAssertions;

using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class AccountAlreadyExistsTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = default!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation,
                PagePath.CompaniesHouseNumber,PagePath.ConfirmCompanyDetails,PagePath.AccountAlreadyExists
            },
             CompaniesHouseSession = new CompaniesHouseSession
             {
                 Company = new Core.Services.Dto.Company.Company
                 {
                      AccountCreatedOn= DateTime.UtcNow
                 }
             }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task GivenAccountAlreadyExists_WhenAccountAlreadyExistsCalled_ThenAccountAlreadyExistsPageReturnedWithAccountCreationDate()
    {
        //Act
        var result = await _systemUnderTest.AccountAlreadyExists();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<AccountAlreadyExistsViewModel>();

        var accountAlreadyExistsViewModel = (AccountAlreadyExistsViewModel)viewResult.Model!;

        accountAlreadyExistsViewModel.DateCreated.Should().Be(_accountCreationSessionMock.CompaniesHouseSession?.Company.AccountCreatedOn?.Date);

        accountAlreadyExistsViewModel.DisplayDateCreated.Should().Be(accountAlreadyExistsViewModel.DateCreated.ToString("d MMMM yyyy"));

        AssertBackLink(viewResult, PagePath.ConfirmCompanyDetails);
    }

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenAccountAlreadyExistsCalled_ThenAccountAlreadyExistsnPageReturned_WithConfirmCompanyDetailsAsTheBackLink()
    {
        //Act
        var result = await _systemUnderTest.AccountAlreadyExists();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().NotBeNull();

        AssertBackLink(viewResult, PagePath.ConfirmCompanyDetails);
    }
}

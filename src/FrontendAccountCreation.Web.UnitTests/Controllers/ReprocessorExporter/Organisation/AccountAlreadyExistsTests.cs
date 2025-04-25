using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class AccountAlreadyExistsTests : OrganisationTestBase
{
    private OrganisationSession? _organisationSession;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.AccountAlreadyExists
            ],
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = new Core.Services.Dto.Company.Company
                {
                    AccountCreatedOn = DateTime.UtcNow
                }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
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

        accountAlreadyExistsViewModel.DateCreated.Should().Be(_organisationSession.ReExCompaniesHouseSession?.Company.AccountCreatedOn?.Date);

        accountAlreadyExistsViewModel.DisplayDateCreated.Should().Be(accountAlreadyExistsViewModel.DateCreated.ToString("d MMMM yyyy"));
    }

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenAccountAlreadyExistsCalled_ThenAccountAlreadyExistsPageReturned_WithConfirmCompanyDetailsAsTheBackLink()
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

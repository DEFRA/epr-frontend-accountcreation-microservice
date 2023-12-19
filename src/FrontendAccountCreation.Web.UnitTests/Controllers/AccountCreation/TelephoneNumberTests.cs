using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class TelephoneNumberTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber
            },
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            CompaniesHouseSession = new CompaniesHouseSession(),
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
        
        _httpContextMock.Setup(x=> x.User.Claims).Returns(new List<Claim>
        {
            new ("emails", "abc@efg.com")
        }.AsEnumerable());
    }

    [TestMethod]
    public async Task TelephoneNumber_PageIsSavedWithValidPhoneNumber_RedirectsToCheckYourDetails_AndUpdateSession()
    {
        // Arrange
        var request = new TelephoneNumberViewModel() { TelephoneNumber = "020 1234 5678" };

        // Act
        var result = await _systemUnderTest.TelephoneNumber(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.CheckYourDetails));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()),
            Times.Once);
    }

    [TestMethod]
    public async Task TelephoneNumber_PageIsSavedWithInvalidPhoneNumber_ReturnsViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(TelephoneNumberViewModel.TelephoneNumber),
            "Field is invalid");

        var request = new TelephoneNumberViewModel() { TelephoneNumber = "" };

        // Act
        var result = await _systemUnderTest.TelephoneNumber(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<TelephoneNumberViewModel>();

        _sessionManagerMock.Verify(
            x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);

        AssertBackLink(viewResult, PagePath.FullName);
    }

    [TestMethod]
    public async Task TelephoneNumber_TelephoneNumberPageIsExited_BackLinkIsFullName()
    {
        //Act
        var result = await _systemUnderTest.TelephoneNumber();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TelephoneNumberViewModel>();
        AssertBackLink(viewResult, PagePath.FullName);
    }

    [TestMethod]
    public async Task TelephoneNumber_UserTravelsFromCheckYourDetailsUsingBackLink_BackLinkIsFullName()
    {
        //Arrange
        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber,
                PagePath.CheckYourDetails
            },
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            CompaniesHouseSession = new CompaniesHouseSession(),
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.TelephoneNumber(false);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TelephoneNumberViewModel>();
        AssertBackLink(viewResult, PagePath.FullName);
    }

    [TestMethod]
    public async Task TelephoneNumber_UserTravelsFromCheckYourDetailsUsingChangeLink_BackLinkIsCheckYourDetails()
    {
        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber
            },
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            CompaniesHouseSession = new CompaniesHouseSession(),
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.TelephoneNumber(true);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TelephoneNumberViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);
    }
}

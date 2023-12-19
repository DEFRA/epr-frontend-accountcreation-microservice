using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class FullNameTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.CompaniesHouseNumber, PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName },
            CompaniesHouseSession = new CompaniesHouseSession(),
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task GivenFullNameProvided_WhenFullNamePagePosted_ThenRedirectsToTelephoneNumberPage_AndUpdateSession()
    {
        // Arrange
        var request = new FullNameViewModel() { FirstName = "John", LastName = "Smith" };

        // Act
        var result = await _systemUnderTest.FullName(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.TelephoneNumber));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GivenNoFirstName_WhenFullNamePagePosted_ThenReturnViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(FullNameViewModel.FirstName),
            "Field is required");
        
        var request = new FullNameViewModel() { LastName = "Smith" };

        // Act
        var result = await _systemUnderTest.FullName(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<FullNameViewModel>();

        _sessionManagerMock.Verify(
            x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
    }
    
    [TestMethod]
    public async Task GivenNoLastName_WhenFullNamePagePosted_ThenReturnViewWithErrorAndCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(FullNameViewModel.LastName),
            "Field is required");
        
        var request = new FullNameViewModel() { FirstName = "John" };

        // Act
        var result = await _systemUnderTest.FullName(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<FullNameViewModel>();

        _sessionManagerMock.Verify(
            x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.RoleInOrganisation);
    }
    
    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenFullNameCalled_ThenFullNamePageReturned_WithRoleInOrganisationAsTheBackLink()
    {
        //Act
        var result = await _systemUnderTest.FullName();
        
        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<FullNameViewModel>();
        AssertBackLink(viewResult, PagePath.RoleInOrganisation);
    }

    [TestMethod]
    public async Task UserNavigatesToFullNamePage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName,
                PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress, PagePath.UkNation, PagePath.ManualInputRoleInOrganisation,
                PagePath.FullName, PagePath.TelephoneNumber, PagePath.CheckYourDetails
            },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.FullName();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<FullNameViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}
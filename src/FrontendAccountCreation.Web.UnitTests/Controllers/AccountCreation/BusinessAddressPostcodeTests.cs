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
public class BusinessAddressPostcodeTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        
        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName, PagePath.BusinessAddressPostcode }
        };
        
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task GivenPostcode_WhenBusinessAddressPostcodeHttpPostCalled_ThenRedirectToSelectBusinessAddress_AndUpdateSession()
    {
        // Arrange
        var model = new BusinessAddressPostcodeViewModel { Postcode = "NW2 3TB" };

        // Act
        var result = await _systemUnderTest.BusinessAddressPostcode(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.SelectBusinessAddress));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenNoPostcode_WhenBusinessAddressPostcodeHttpPostCalled_ThenReturnViewWithErrorAndCorrectBackLink()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(BusinessAddressPostcodeViewModel.Postcode), "Postcode field is required");

        // Act
        var result = await _systemUnderTest.BusinessAddressPostcode(new BusinessAddressPostcodeViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();
        
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<BusinessAddressPostcodeViewModel>();
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.TradingName);
    }
    
    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenBusinessAddressPostcodeCalled_ThenBusinessAddressPostcodePageReturned_WithTradingNameAsTheBackLink()
    {
        //Act
        var result = await _systemUnderTest.BusinessAddressPostcode();
        
        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<BusinessAddressPostcodeViewModel>();
        AssertBackLink(viewResult, PagePath.TradingName);
    }
}

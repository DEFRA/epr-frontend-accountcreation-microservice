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
public class ManualInputRoleInOrganisationTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        
        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation, PagePath.TradingName,
                                         PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress, PagePath.UkNation, PagePath.ManualInputRoleInOrganisation },
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task ManualInputRoleInOrganisation_RoleInOrganisationSavedAsOwner_RedirectsToFullNamePageAndUpdateSession()
    {
        // Arrange
        var request = new ManualInputRoleInOrganisationViewModel { RoleInOrganisation = "Owner" };

        // Act
        var result = await _systemUnderTest.ManualInputRoleInOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.FullName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task ManualInputRoleInOrganisation_RoleInOrganisationSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsUkNation()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(ManualInputRoleInOrganisationViewModel.RoleInOrganisation), "Role in organisation field is required");

        // Act
        var result = await _systemUnderTest.ManualInputRoleInOrganisation(new ManualInputRoleInOrganisationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<ManualInputRoleInOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.UkNation);
    }

    [TestMethod]
    public async Task ManualInputRoleInOrganisation_ManualInputRoleInOrganisationPageIsExited_BackLinkIsUkNation()
    {
        //Act
        var result = await _systemUnderTest.ManualInputRoleInOrganisation();
        
        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ManualInputRoleInOrganisationViewModel>();
        AssertBackLink(viewResult, PagePath.UkNation);
    }

    [TestMethod]
    public async Task UserNavigatesToManualInputRoleInOrganisationPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
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
        var result = await _systemUnderTest.ManualInputRoleInOrganisation();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ManualInputRoleInOrganisationViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}

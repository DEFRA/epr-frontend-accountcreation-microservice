using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;


[TestClass]
public class RegisteredWithCompaniesHouseTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_OrganisationIsRegistered_RedirectsToCompaniesHouseNumberPage_AndUpdateSession()
    {
        // Arrange
        var request = new RegisteredWithCompaniesHouseViewModel { IsTheOrganisationRegistered = YesNoAnswer.Yes };

        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.CompaniesHouseNumber));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_OrganisationIsNotRegistered_RedirectsToTypeOfOrganisationPage_AndUpdateSession()
    {
        // Arrange
        var request = new RegisteredWithCompaniesHouseViewModel { IsTheOrganisationRegistered = YesNoAnswer.No };

        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.TypeOfOrganisation));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_PageIsSavedWithNoAnswer_ReturnsViewWithError()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(RegisteredWithCompaniesHouseViewModel.IsTheOrganisationRegistered), "Field is required");

        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse(new RegisteredWithCompaniesHouseViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<RegisteredWithCompaniesHouseViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_OrganisationIsRegistered_RedirectsToViewResult()
    {
        // Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredWithCompaniesHouse }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(accountCreationSessionMock);


        // Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task RegisteredWithCompaniesHouse_RegisteredWithCompaniesHousePageIsExited_BackLinkIsRegisteredAsCharity()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse
            },
            IsUserChangingDetails = false,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredWithCompaniesHouseViewModel>();
        AssertBackLink(viewResult, PagePath.RegisteredAsCharity);
    }

    [TestMethod]
    public async Task UserNavigatesToRegisteredWithCompaniesHousePage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation,PagePath.TradingName,
                PagePath.ConfirmCompanyDetails, PagePath.RoleInOrganisation, PagePath.FullName, PagePath.TelephoneNumber,
                PagePath.CheckYourDetails
            },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.RegisteredWithCompaniesHouse();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<RegisteredWithCompaniesHouseViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}
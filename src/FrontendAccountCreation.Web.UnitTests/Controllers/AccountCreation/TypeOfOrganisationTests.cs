﻿using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.AccountCreation;
using FrontendAccountCreation.Web.ViewModels.AccountCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class TypeOfOrganisationTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null! ;
    
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        
        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation},
            CompaniesHouseSession = new CompaniesHouseSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    public async Task TypeOfOrganisation_OrganisationTypeSavedAsSoleTrader_RedirectsToTradingNamePage_AndUpdateSession()
    {
        // Arrange
        var request = new TypeOfOrganisationViewModel() { ProducerType = ProducerType.SoleTrader };

        // Act
        var result = await _systemUnderTest.TypeOfOrganisation(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountCreationController.TradingName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<AccountCreationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task TypeOfOrganisation_OrganisationTypeSavedWithNoAnswer_ReturnsViewWithErrorAndBackLinkIsRegisteredWithCompaniesHouse()
    {
        // Arrange
        _systemUnderTest.ModelState.AddModelError(nameof(TypeOfOrganisationViewModel.ProducerType), "Field is required");

        // Act
        var result = await _systemUnderTest.TypeOfOrganisation(new TypeOfOrganisationViewModel());

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<TypeOfOrganisationViewModel>();

        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<AccountCreationSession>>()), Times.Never);
        AssertBackLink(viewResult, PagePath.RegisteredWithCompaniesHouse);
    }

    [TestMethod]
    public async Task TypeOfOrganisation_TypeOfOrganisationPageIsExited_BackLinkIsRegisteredWithCompaniesHouse()
    {
        //Act
        var result = await _systemUnderTest.TypeOfOrganisation();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TypeOfOrganisationViewModel>();
        AssertBackLink(viewResult, PagePath.RegisteredWithCompaniesHouse);
    }

    [TestMethod]
    public async Task UserNavigatesToTypeOfOrganisationPage_FromCheckYourDetailsPage_BackLinkShouldBeCheckYourDetails()
    {
        //Arrange
        var accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string>
            {
                PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.TypeOfOrganisation
            },
            IsUserChangingDetails = true,
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(accountCreationSessionMock);

        //Act
        var result = await _systemUnderTest.TypeOfOrganisation();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<TypeOfOrganisationViewModel>();
        AssertBackLink(viewResult, PagePath.CheckYourDetails);

    }
}

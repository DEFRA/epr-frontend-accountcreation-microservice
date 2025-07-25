﻿using Azure.Core;
using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class BusinessAddressTests : OrganisationTestBase
{
    private OrganisationSession _organisationSession = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _organisationSession = new OrganisationSession
        {
            Journey =
            [
                PagePath.UkNation, PagePath.BusinessAddress
            ],
            ReExManualInputSession = new ReExManualInputSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_organisationSession);
    }

    [TestMethod]
    public async Task GET_WhenBusinessAddressIsNotInSession_ThenViewIsReturnedWithoutDetails()
    {
        //Act
        var result = await _systemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExBusinessAddressViewModel>();
        var viewModel = (ReExBusinessAddressViewModel?)viewResult.Model;
        viewModel!.BuildingNumber.Should().BeNull();
        viewModel.BuildingName.Should().BeNull();
        viewModel.Street.Should().BeNull();
        viewModel.Town.Should().BeNull();
        viewModel.County.Should().BeNull();
        viewModel.Postcode.Should().BeNull();
    }

    [TestMethod]
    public async Task GET_WhenBusinessAddressIsInSession_ThenViewIsReturnedWithBusinessAddress()
    {
        //Arrange
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            BusinessAddress = new Core.Addresses.Address
            {
                BuildingName = "building name",
                BuildingNumber = "building number",
                Street = "street",
                Town = "town",
                County = "county",
                Postcode = "b1 2AA",
                IsManualAddress = true
            }
        };

        //Act
        var result = await _systemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<ReExBusinessAddressViewModel>();
        var viewModel = (ReExBusinessAddressViewModel?)viewResult.Model;
        viewModel!.BuildingName.Should().Be("building name");
        viewModel.BuildingNumber.Should().Be("building number");
        viewModel.Street.Should().Be("street");
        viewModel.Town.Should().Be("town");
        viewModel.County.Should().Be("county");
        viewModel.Postcode.Should().Be("b1 2AA");
    }

    [TestMethod]
    public async Task GET_ThenBackLinkIsCorrect()
    {
        //Act
        var result = await _systemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.UkNation);
    }

    [TestMethod]
    public async Task POST_GivenBusinessAddress_ThenRedirectToNextPage()
    {
        // Arrange
        var request = new ReExBusinessAddressViewModel();

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.SoleTrader));
    }

    [TestMethod]
    public async Task POST_GivenBusinessAddress_ThenUpdatesSession()
    {
        // Arrange
        var request = new ReExBusinessAddressViewModel();

        // Act
        await _systemUnderTest.BusinessAddress(request);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }

    [TestMethod]
    public async Task POST_GivenBusinessAddressDetailsMissing_ThenSessionNotUpdated()
    {
        // Arrange
        var request = new ReExBusinessAddressViewModel();

        _systemUnderTest.ModelState.AddModelError(nameof(ReExBusinessAddressViewModel.Town), "Enter your organisation's town or city");

        // Act
        await _systemUnderTest.BusinessAddress(request);

        // Assert
        _sessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<OrganisationSession>>()),
            Times.Never);
    }

    [TestMethod]
    public async Task POST_GivenBusinessAddressDetailsMissing_ThenReturnView()
    {
        // Arrange
        var request = new ReExBusinessAddressViewModel();

        _systemUnderTest.ModelState.AddModelError(nameof(ReExBusinessAddressViewModel.Town), "Enter your organisation's town or city");

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task POST_GivenBusinessAddressDetailsMissing_ThenViewHasCorrectBackLink()
    {
        // Arrange
        var request = new ReExBusinessAddressViewModel();

        _systemUnderTest.ModelState.AddModelError(nameof(ReExBusinessAddressViewModel.Town), "Enter your organisation's town or city");

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, PagePath.UkNation);
    }

    [TestMethod]
    public async Task POST_GivenFieldTooLong_ThenReturnViewWithUsersBadInput()
    {
        // Arrange
        const string tooLongTown = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 1";

        var request = new ReExBusinessAddressViewModel
        {
            Town = tooLongTown
        };

        _systemUnderTest.ModelState.AddModelError(nameof(ReExBusinessAddressViewModel.Town), "Town or city must be 70 characters or less");

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<ReExBusinessAddressViewModel?>();
        var resultViewModel = (ReExBusinessAddressViewModel?)viewResult.Model;
        resultViewModel!.Town.Should().Be(tooLongTown);
    }

    [TestMethod]
    public async Task POST_BusinessAddress_WhenGivenPartnership_ThenRedirectToPartnershipTypePage()
    {
        // Arrange
        var request = new ReExBusinessAddressViewModel();
        _organisationSession.ReExManualInputSession.ProducerType = ProducerType.Partnership;

        // Act
        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(LimitedPartnershipController.NonCompaniesHousePartnershipType));
    }


    [TestMethod]
    public async Task POST_WhenNonCompaniesHouseAndUnincorporatedBody_ThenNavigateToUnincorporatedFlow()
    {
        //Arrange
        var request = new ReExBusinessAddressViewModel();

        _organisationSession.OrganisationType = OrganisationType.NonCompaniesHouseCompany;
        _organisationSession.ReExManualInputSession = new ReExManualInputSession
        {
            BusinessAddress = new Core.Addresses.Address
            {
                BuildingName = "building name",
                BuildingNumber = "building number",
                Street = "street",
                Town = "town",
                County = "county",
                Postcode = "b1 2AA",
                IsManualAddress = true,
            },
            ProducerType = ProducerType.UnincorporatedBody
        };

        var result = await _systemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.UnincorporatedRoleInOrganisation));
    }
}
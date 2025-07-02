using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Unincorporated;

[TestClass]
public class CheckYourDetailsTests : UnincorporatedTestBase
{
    private const string OrganisationName = "TestOrganisationName";


    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task CheckYourDetails_Get_ReturnsPage()
    {
        // Arrange
        SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.CheckYourDetails();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        var model = (ReExUnincorporatedCheckYourDetailsViewModel)viewResult.Model;
        model.JobTitle.Should().Be(nameof(RoleInOrganisation.Director));
        model.TradingName.Should().Be(OrganisationName);
        model.Nation.Should().Be(Nation.England);

        AssertBackLink(viewResult, PagePath.UnincorporatedApprovedPerson);
    }

    [TestMethod]
    public async Task CheckYourDetails_Post_ContinuePressed_RedirectsToOrganisationControllerDeclaration()
    {
        // Arrange
        var session = SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.CheckYourDetails(null);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(OrganisationController.Declaration), redirectResult.ActionName);
        _sessionManagerMock.Verify(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once());
    }

    [TestMethod]
    public async Task CheckYourDetails_Post_InvitePressed_RedirectsToTeamMemberDetails()
    {
        // Arrange
        var session = SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.CheckYourDetails(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(UnincorporatedController.TeamMemberDetails), redirectResult.ActionName);
        _sessionManagerMock.Verify(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once());
    }

    private OrganisationSession SetupOrganisationSession()
    {
        var session = new OrganisationSession
        {
            Journey =
            [
                PagePath.BusinessAddress,
                PagePath.UnincorporatedManageControl,
                PagePath.UnincorporatedManageAccountPerson,
                PagePath.UnincorporatedApprovedPerson,
                PagePath.UnincorporatedCheckYourDetails
            ],
            ReExUnincorporatedFlowSession = new ReExUnincorporatedFlowSession(),
            ReExManualInputSession = new ReExManualInputSession
            {
                BusinessAddress = new Address
                {
                    BuildingName = "testBuildingName",
                    BuildingNumber = "testBuildingNumber",
                    Street = "testStreet",
                    Town = "testTown",
                    County = "testCounty",
                    Country = "testCountry",
                    Postcode = "TE1 1ST"
                },
                OrganisationName = OrganisationName,
                RoleInOrganisation = RoleInOrganisation.Director
            },
            UkNation = Nation.England
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        return session;
    }
}
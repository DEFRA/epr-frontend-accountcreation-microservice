using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class LimitedPartnershipControllerTests : LimitedPartnershipTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.IsPartnership,
                PagePath.PartnershipType,
                PagePath.LimitedPartnershipType,
                PagePath.LimitedPartnershipNamesOfPartners,
                PagePath.LimitedPartnershipCheckNamesOfPartners
            },
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new ReExPartnership
                {
                    IsLimitedPartnership = true,
                    LimitedPartnership = new ReExTypesOfPartner()
                }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    [DataRow("", "check-your-details")]
    [DataRow("MyPage", "check-your-details")]
    [DataRow("check-your-details", "")]
    public void SetBackLink_When_IsUserChangingDetails_Is_True_Updates_ViewBag(string currentPagePath, string expectedValue)
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = true;

        // Act
        _systemUnderTest.SetBackLink(_orgSessionMock, currentPagePath);

        // Assert
        _systemUnderTest.ViewData["BackLinkToDisplay"].ToString().Should().Be(expectedValue);
    }

    [TestMethod]
    [DataRow("", "")]
    [DataRow("MyPage", "")]
    [DataRow(PagePath.LimitedPartnershipCheckNamesOfPartners, PagePath.LimitedPartnershipNamesOfPartners)]
    [DataRow(PagePath.LimitedPartnershipNamesOfPartners, PagePath.LimitedPartnershipType)]
    [DataRow(PagePath.LimitedPartnershipType, PagePath.PartnershipType)]
    [DataRow(PagePath.PartnershipType, PagePath.IsPartnership)]
    [DataRow(PagePath.IsPartnership, "")]
    public void SetBackLink_When_IsUserChangingDetails_Is_False_Updates_ViewBag(string currentPagePath, string expectedValue)
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = false;

        // Act
        _systemUnderTest.SetBackLink(_orgSessionMock, currentPagePath);

        // Assert
        _systemUnderTest.ViewData["BackLinkToDisplay"].ToString().Should().Be(expectedValue);
    }

    [TestMethod]
    public async Task SaveSessionAndRedirect_WhenGivenAction_Flags_IsUserChangingDetails_False()
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = true;

        // Act
        await _systemUnderTest.SaveSessionAndRedirect(_orgSessionMock, string.Empty, string.Empty, null);

        // Assert
        _orgSessionMock.IsUserChangingDetails.Should().BeFalse();
    }

    [TestMethod]
    public async Task SaveSessionAndRedirect_WhenGivenAction_Redirects()
    {
        // Act
        var result = await _systemUnderTest.SaveSessionAndRedirect(_orgSessionMock, "myAction", string.Empty, null);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be("myAction");
        redirectToActionResult.ControllerName.Should().BeNull();
        redirectToActionResult.RouteValues.Should().BeNull();
    }

    [TestMethod]
    public async Task SaveSessionAndRedirect_WhenGivenAction_AndController_Flags_IsUserChangingDetails_False()
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = true;

        // Act
        await _systemUnderTest.SaveSessionAndRedirect(_orgSessionMock, string.Empty, string.Empty, string.Empty, null);

        // Assert
        _orgSessionMock.IsUserChangingDetails.Should().BeFalse();
    }

    [TestMethod]
    public async Task SaveSessionAndRedirect_WhenGivenAction_AndController_Redirects()
    {
        // Act
        var result = await _systemUnderTest.SaveSessionAndRedirect(_orgSessionMock, "herController", "myAction", string.Empty, null);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be("myAction");
        redirectToActionResult.ControllerName.Should().Be("her");
        redirectToActionResult.RouteValues.Should().BeNull();
    }

    [TestMethod]
    public async Task SaveSessionAndRedirect_WhenGivenAction_AndController_AndQueryString_Flags_IsUserChangingDetails_False()
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = true;

        // Act
        await _systemUnderTest.SaveSessionAndRedirect(_orgSessionMock, string.Empty, string.Empty, null, null, null);

        // Assert
        _orgSessionMock.IsUserChangingDetails.Should().BeFalse();
    }

    [TestMethod]
    public async Task SaveSessionAndRedirect_WhenGivenAction_AndBlankControllerr_AndQueryString_Redirects()
    {
        // Arrange
        var querystring = new KeyValuePair<string, string>("x", "y") as object;

        // Act
        var result = await _systemUnderTest.SaveSessionAndRedirect(_orgSessionMock, "myAction", string.Empty, null, null, querystring);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be("myAction");
        redirectToActionResult.ControllerName.Should().BeNull();
        redirectToActionResult.RouteValues.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task SaveSessionAndRedirect_WhenGivenAction_AndControllerr_AndQueryString_Redirects()
    {
        // Arrange
        var querystring = new KeyValuePair<string, string>("x", "y") as object;

        // Act
        var result = await _systemUnderTest.SaveSessionAndRedirect(_orgSessionMock, "myAction", string.Empty, null, "herController", querystring);

        // Assert
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Which;
        redirectToActionResult.ActionName.Should().Be("myAction");
        redirectToActionResult.ControllerName.Should().Be("her");
        redirectToActionResult.RouteValues.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task SaveSession_UpdatessJourneyInSession()
    {
        // Arrange
        string currentPagePath = PagePath.LimitedPartnershipCheckNamesOfPartners;
        string nextPath = "MyNextPage";

        // Act
        await _systemUnderTest.SaveSession(_orgSessionMock, currentPagePath, nextPath);

        // Assert
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), _orgSessionMock), Times.Once());

        _orgSessionMock.Journey.Should().ContainInOrder(currentPagePath, nextPath);
    }
}
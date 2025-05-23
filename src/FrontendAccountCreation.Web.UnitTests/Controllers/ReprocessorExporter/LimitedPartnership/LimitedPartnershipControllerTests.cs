using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using Moq;
using Microsoft.AspNetCore.Http;
using FluentAssertions;

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
                    LimitedPartnership = new ReExLimitedPartnership()
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
}

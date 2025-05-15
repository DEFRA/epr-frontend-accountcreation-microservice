using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.Sessions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class NamesOfPartnersTests : LimitedPartnershipTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey = new List<string>
            {
                PagePath.LimitedPartnershipType,
            },
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsPartnership = true,
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
    public async Task NamesOfPartners_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.NamesOfPartners();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<LimitedPartnershipPartnersViewModel>();

        ((LimitedPartnershipPartnersViewModel)viewResult.Model).Partners.Should().HaveCount(1);
    }
}
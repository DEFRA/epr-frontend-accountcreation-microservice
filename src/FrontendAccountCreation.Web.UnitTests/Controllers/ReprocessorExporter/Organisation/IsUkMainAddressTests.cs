using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class IsUkMainAddressTests() : YesNoPageTestBase<IsUkMainAddressViewModel>(
    c => c.IsUkMainAddress(),
    (c, vm) => c.IsUkMainAddress(vm),
    (session, val) => session.IsUkMainAddress = val,
    session => session.IsUkMainAddress,
    vm => vm.IsUkMainAddress)
{
    // Page and Journey details
    protected override string CurrentPagePath => PagePath.IsUkMainAddress;
    protected override string ExpectedBacklinkPagePath => PagePath.RegisteredWithCompaniesHouse;

    protected override List<string> JourneyForGetBacklinkTest =>
    [
        PagePath.RegisteredAsCharity, PagePath.RegisteredWithCompaniesHouse, PagePath.IsUkMainAddress
    ];

    // Redirect targets
    protected override string RedirectActionNameOnYes => nameof(OrganisationController.OrganisationName);
    protected override string RedirectActionNameOnNo => nameof(OrganisationController.OrganisationName);

    [TestMethod]
    [DataRow(YesNoAnswer.Yes)]
    [DataRow(YesNoAnswer.No)]
    public async Task IsUkMainAddress_Redirects_To_OrganisationName_With_IsUkMainAddressAs(YesNoAnswer answer)
    {
        // Arrange
        var orgCreationSessionMock = new OrganisationSession
        {
            Journey = [PagePath.RegisteredWithCompaniesHouse, PagePath.OrganisationName],
            TradingName = "Test Trading Name",
            UkNation = Nation.England,
            IsIndividualInCharge = true,
            AreTheyIndividualInCharge = false,
            UserManagesOrControls = YesNoNotSure.Yes,
            TheyManageOrControlOrganisation = YesNoNotSure.NotSure,            
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.SoleTrader,
                BusinessAddress = new Address
                {
                    Street = "123 Test Street",
                    Postcode = "AB12 3CD"
                },
                TeamMembers =
                [
                    new ReExCompanyTeamMember
                    {
                        FullName = "Test User",
                        Email = "Test@test.com"
                    }
                ]
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgCreationSessionMock);

        var request = new IsUkMainAddressViewModel { IsUkMainAddress = answer };

        // Act
        var result = await _systemUnderTest.IsUkMainAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(OrganisationController.OrganisationName));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()), Times.Once);
    }
}
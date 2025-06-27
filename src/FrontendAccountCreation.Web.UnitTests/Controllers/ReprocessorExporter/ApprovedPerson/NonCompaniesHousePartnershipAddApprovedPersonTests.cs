using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class NonCompaniesHousePartnershipAddApprovedPersonTests : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock = null!;

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
                PagePath.LimitedPartnershipRole,
                PagePath.AddAnApprovedPerson
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
    public async Task NonCompaniesHousePartnershipAddApprovedPerson_Get_ReturnsViewWithCorrectModel_WhenNotPartnership()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = false;

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipAddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<AddApprovedPersonViewModel>();

        var model = (AddApprovedPersonViewModel)viewResult.Model;
        model.IsNonCompaniesHousePartnership.Should().BeFalse();
    }


    [TestMethod]
    public async Task NonCompaniesHousePartnershipAddApprovedPerson_Get_ReturnsViewWithCorrectModel()
    {
        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipAddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<AddApprovedPersonViewModel>();

        var model = (AddApprovedPersonViewModel)viewResult.Model;
        model.IsNonCompaniesHousePartnership.Should().BeTrue();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipAddApprovedPerson_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();
        _systemUnderTest.ModelState.AddModelError("Test", "Required");
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession();

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipAddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task Post_BeAnApprovedPersonOption_RedirectsToYouAreApprovedPerson()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = nameof(InviteUserOptions.BeAnApprovedPerson)
        };

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipAddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("YouAreApprovedPerson");
        _orgSessionMock.IsApprovedUser.Should().BeTrue();
    }

}


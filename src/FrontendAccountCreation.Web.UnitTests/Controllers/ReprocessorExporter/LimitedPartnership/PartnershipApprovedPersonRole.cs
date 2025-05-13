using FluentAssertions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership.ApprovedPersons;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class ApprovedPersonPartnershipRoleTests : LimitedPartnershipTestBase
{
    private Guid _approvedPersonId;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _approvedPersonId = Guid.NewGuid();

        _orgSessionMock = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new ReExPartnership
                {
                    LimitedPartnership = new ReExLimitedPartnership
                    {
                        //PartnershipApprovedPersons = new List<ReExLimitedPartnershipApprovedPerson>
                        //{
                        //    new()
                        //    {
                        //        Id = _approvedPersonId,
                        //        Role = ReExLimitedPartnershipRoles.Director
                        //    }
                        //}
                    }
                }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task Get_WithExistingId_ReturnsViewWithRole()
    {
        var result = await _systemUnderTest.ApprovedPersonPartnershipRole(_approvedPersonId);

        result.Should().BeOfType<ViewResult>();
        var model = ((ViewResult)result).Model.Should().BeOfType<LimitedPartnershipApprovedPersonRoleViewModel>().Subject;
        model.RoleInOrganisation.Should().Be(ReExLimitedPartnershipRoles.Director);
    }

    [TestMethod]
    public async Task Get_WithNonExistentId_ReturnsEmptyView()
    {
        var result = await _systemUnderTest.ApprovedPersonPartnershipRole(Guid.NewGuid());

        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).Model.Should().BeNull();
    }

    [TestMethod]
    public async Task Post_WithInvalidModel_ReturnsSameView()
    {
        var model = new LimitedPartnershipApprovedPersonRoleViewModel { Id = _approvedPersonId };
        _systemUnderTest.ModelState.AddModelError("RoleInOrganisation", "Required");

        var result = await _systemUnderTest.ApprovedPersonPartnershipRole(model);

        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).Model.Should().Be(model);
    }

    [TestMethod]
    public async Task Post_WithRoleNone_RedirectsToCannotBeInvited()
    {
        var model = new LimitedPartnershipApprovedPersonRoleViewModel
        {
            Id = _approvedPersonId,
            RoleInOrganisation = ReExLimitedPartnershipRoles.None
        };

        var result = await _systemUnderTest.ApprovedPersonPartnershipRole(model);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(_systemUnderTest.PersonCanNotBeInvited));
    }

    [TestMethod]
    public async Task Post_WithValidRole_RedirectsToDetails()
    {
        var model = new LimitedPartnershipApprovedPersonRoleViewModel
        {
            Id = _approvedPersonId,
            RoleInOrganisation = ReExLimitedPartnershipRoles.CompanySecretary
        };

        var result = await _systemUnderTest.ApprovedPersonPartnershipRole(model);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(_systemUnderTest.ApprovedPersonDetails));
    }

    [TestMethod]
    public async Task Post_WithMissingIdInSession_DoesNotThrow()
    {
        var model = new LimitedPartnershipApprovedPersonRoleViewModel
        {
            Id = Guid.NewGuid(),
            RoleInOrganisation = ReExLimitedPartnershipRoles.CompanySecretary
        };

        var result = await _systemUnderTest.ApprovedPersonPartnershipRole(model);

        result.Should().BeOfType<RedirectToActionResult>();
    }
}
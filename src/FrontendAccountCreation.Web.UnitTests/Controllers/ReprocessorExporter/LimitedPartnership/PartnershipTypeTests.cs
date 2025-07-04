using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership;

[TestClass]
public class PartnershipTypeTests : LimitedPartnershipTestBase
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
    public async Task PartnershipType_Get_ReturnsView()
    {
        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<PartnershipTypeRequestViewModel>();
    }

    [TestMethod]
    public async Task PartnershipType_Get_WithExistingPartnership_ReturnsViewWithCorrectType()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = true;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.TypeOfPartnership.Should().Be(Core.Sessions.PartnershipType.LimitedPartnership);
    }

    [TestMethod]
    public async Task PartnershipType_Get_WhenCompaniesHouseSessionIsNull_ReturnsViewWithNullPartnershipType()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession = null;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.TypeOfPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task PartnershipType_Get_WhenPartnershipSessionIsNull_ReturnsViewWithNullPartnershipType()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.TypeOfPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task PartnershipType_Get_SetsBackLink_WhenNotChangingDetails()
    {
        // Arrange
        _orgSessionMock.Journey = new List<string>
        {
            PagePath.TypeOfOrganisation,
            PagePath.PartnershipType
        };
        _orgSessionMock.IsUserChangingDetails = false;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.TypeOfOrganisation);
    }

    [TestMethod]
    public async Task PartnershipType_Get_WhenUserChangingDetails_SetsCheckYourDetailsBackLink()
    {
        // Arrange
        _orgSessionMock.IsUserChangingDetails = true;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.CheckYourDetails);
    }

    [TestMethod]
    public async Task PartnershipType_Get_WithLimitedLiabilityPartnership_ReturnsViewWithCorrectType()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = new ReExPartnership
        {
            IsLimitedLiabilityPartnership = true
        };

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.TypeOfPartnership.Should().Be(Core.Sessions.PartnershipType.LimitedLiabilityPartnership);
    }

    [TestMethod]
    public async Task PartnershipType_Get_WhenReExCompaniesHouseSessionIsNull_ReturnsViewWithNullPartnershipType()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession = null;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.TypeOfPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task PartnershipType_Get_WhenReExPartnershipSessionIsNull_ReturnsViewWithNullPartnershipType()
    {
        // Arrange
        _orgSessionMock.ReExCompaniesHouseSession.Partnership = null;

        // Act
        var result = await _systemUnderTest.PartnershipType();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as PartnershipTypeRequestViewModel;
        model.Should().NotBeNull();
        model!.TypeOfPartnership.Should().BeNull();
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel();
        _systemUnderTest.ModelState.AddModelError("isLimitedPartnership", "Required");

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(model);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithValidModel_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = Core.Sessions.PartnershipType.LimitedPartnership
        };

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.LimitedPartnershipType));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.IsOrganisationAPartnership.HasValue &&
                s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership &&
                s.ReExCompaniesHouseSession.Partnership.LimitedPartnership != null &&
                s.ReExCompaniesHouseSession.ProducerType.HasValue &&
                s.ReExCompaniesHouseSession.ProducerType == ProducerType.LimitedPartnership
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithLimitedLiabilityPartnership_UpdatesSessionAndRedirects()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = PartnershipType.LimitedLiabilityPartnership
        };

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.LimitedLiabilityPartnership));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership.IsLimitedLiabilityPartnership &&
                s.ReExCompaniesHouseSession.ProducerType.HasValue &&
                s.ReExCompaniesHouseSession.ProducerType == ProducerType.LimitedLiabilityPartnership
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WithLimitedPartnershipAndNullPartnership_CreatesNewPartnership()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = Core.Sessions.PartnershipType.LimitedPartnership
        };

        var session = new OrganisationSession
        {
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = null
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.PartnershipType(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;
        redirectResult.ActionName.Should().Be(nameof(_systemUnderTest.LimitedPartnershipType));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(
            It.IsAny<ISession>(),
            It.Is<OrganisationSession>(s =>
                s.ReExCompaniesHouseSession.Partnership != null &&
                s.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership == true
            )),
            Times.Once);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WhenTypeNotPresentInSession_DoesNotClearSession()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = Core.Sessions.PartnershipType.LimitedPartnership
        };

        _orgSessionMock = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            InviteUserOption = InviteUserOptions.InviteAnotherPerson,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = [],
                Partnership = new(),
                RoleInOrganisation = RoleInOrganisation.CompanySecretary,
                IsInEligibleToBeApprovedPerson = true,
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock).Verifiable();

        // Act
        await _systemUnderTest.PartnershipType(model);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.Should().NotBeNull();
        _orgSessionMock.InviteUserOption.Should().NotBeNull().And.Be(InviteUserOptions.InviteAnotherPerson);
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().NotBeNull().And.Be(RoleInOrganisation.CompanySecretary);
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().Be(true);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WhenTypeStaysAsLimitedPartnership_DoesNotClearLpSession()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = Core.Sessions.PartnershipType.LimitedPartnership
        };

        _orgSessionMock = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            InviteUserOption = InviteUserOptions.InviteAnotherPerson,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = [],
                Partnership = new ReExPartnership { IsLimitedPartnership = true, IsLimitedLiabilityPartnership = false, LimitedPartnership = new() },
                RoleInOrganisation = RoleInOrganisation.CompanySecretary,
                IsInEligibleToBeApprovedPerson = true,
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock).Verifiable();

        // Act
        await _systemUnderTest.PartnershipType(model);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.Should().NotBeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Should().NotBeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership.Should().BeNull();
        _orgSessionMock.InviteUserOption.Should().NotBeNull().And.Be(InviteUserOptions.InviteAnotherPerson);
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().NotBeNull().And.Be(RoleInOrganisation.CompanySecretary);
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().Be(true);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WhenTypeStaysAsLimitedLiabilityPartnership_DoesNoClearLlpSession()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = Core.Sessions.PartnershipType.LimitedLiabilityPartnership
        };

        _orgSessionMock = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            InviteUserOption = InviteUserOptions.InviteAnotherPerson,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = [],
                Partnership = new ReExPartnership { IsLimitedPartnership = false, IsLimitedLiabilityPartnership = true, LimitedLiabilityPartnership = new() },
                RoleInOrganisation = RoleInOrganisation.CompanySecretary,
                IsInEligibleToBeApprovedPerson = true,
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock).Verifiable();

        // Act
        await _systemUnderTest.PartnershipType(model);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.Should().NotBeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership.Should().NotBeNull();
        _orgSessionMock.InviteUserOption.Should().NotBeNull().And.Be(InviteUserOptions.InviteAnotherPerson);
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().NotBeNull().And.Be(RoleInOrganisation.CompanySecretary);
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().Be(true);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WhenTypeChangesToLimitedLiabilityPartnership_ClearsLpSession()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = Core.Sessions.PartnershipType.LimitedLiabilityPartnership
        };

        _orgSessionMock = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            InviteUserOption = InviteUserOptions.InviteAnotherPerson,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = [],
                Partnership = new ReExPartnership { IsLimitedPartnership = true, IsLimitedLiabilityPartnership = false, LimitedPartnership = new() },
                RoleInOrganisation = RoleInOrganisation.CompanySecretary,
                IsInEligibleToBeApprovedPerson = true,
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock).Verifiable();

        // Act
        await _systemUnderTest.PartnershipType(model);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedPartnership.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.IsLimitedLiabilityPartnership.Should().BeTrue();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership.Should().BeFalse();
        _orgSessionMock.InviteUserOption.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().Be(false);
    }

    [TestMethod]
    public async Task PartnershipType_Post_WhenTypeChangesToLimitedPartnership_ClearsLlpSession()
    {
        // Arrange
        var model = new PartnershipTypeRequestViewModel
        {
            TypeOfPartnership = Core.Sessions.PartnershipType.LimitedPartnership
        };

        _orgSessionMock = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            InviteUserOption = InviteUserOptions.InviteAnotherPerson,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                TeamMembers = [],
                Partnership = new ReExPartnership { IsLimitedPartnership = false, IsLimitedLiabilityPartnership = true, LimitedLiabilityPartnership = new() },
                RoleInOrganisation = RoleInOrganisation.CompanySecretary,
                IsInEligibleToBeApprovedPerson = true,
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock).Verifiable();

        // Act
        await _systemUnderTest.PartnershipType(model);

        // Assert
        _orgSessionMock.ReExCompaniesHouseSession.TeamMembers.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.LimitedLiabilityPartnership.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.IsLimitedPartnership.Should().BeTrue();
        _orgSessionMock.ReExCompaniesHouseSession.Partnership.IsLimitedLiabilityPartnership.Should().BeFalse();
        _orgSessionMock.InviteUserOption.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.RoleInOrganisation.Should().BeNull();
        _orgSessionMock.ReExCompaniesHouseSession.IsInEligibleToBeApprovedPerson.Should().Be(false);
    }
}
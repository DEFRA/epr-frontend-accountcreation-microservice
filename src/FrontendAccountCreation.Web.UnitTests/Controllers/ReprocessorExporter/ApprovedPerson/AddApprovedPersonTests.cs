using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class AddApprovedPersonTests : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.RegisteredAsCharity,
                PagePath.RegisteredWithCompaniesHouse,
                PagePath.CompaniesHouseNumber,
                PagePath.ConfirmCompanyDetails,
                PagePath.RoleInOrganisation,
                "Pagebefore",
                PagePath.TeamMemberRoleInOrganisation
            ],
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession(),
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task AddApprovedPerson_SessionRetrievedAndSaved_ReturnsView()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        _sessionManagerMock.Verify(s => s.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }

    [TestMethod]
    public async Task AddApprovedPerson_ModelStateInvalid_ReturnsViewWithModel()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();
        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType(model.GetType());
    }

    [TestMethod]
    public async Task AddApprovedPerson_IAgreeToBeApprovedPerson_RedirectsToYouAreApprovedPerson()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.BeAnApprovedPerson.ToString()
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(ApprovedPersonController.YouAreApprovedPerson));
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteAnotherApprovedPerson_CompanyHouseFlow_RedirectsToTeamMemberRoleInOrganisation()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany
        };
        
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteAnotherApprovedPerson_CompanyHouseFlow_AddsTeamMemberRoleInOrganisationToJourney()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = nameof(InviteUserOptions.InviteAnotherPerson)
        };

        var session = new OrganisationSession
        {
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            Journey = ["PreviousPage"]
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Capture the session that gets saved
        OrganisationSession? savedSession = null;
        _sessionManagerMock
            .Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<OrganisationSession>()))
            .Callback<ISession, OrganisationSession>((_, s) => savedSession = s)
            .Returns(Task.CompletedTask);

        // Act
        await _systemUnderTest.AddApprovedPerson(model);

        // Assert

        // Verify the session was saved with PagePath.TeamMemberRoleInOrganisation as the last page in the Journey
        savedSession.Should().NotBeNull("Session should have been saved");
        savedSession!.Journey.Should().NotBeNull("Journey should not be null");
        savedSession.Journey.Last().Should().Be(PagePath.TeamMemberRoleInOrganisation, "TeamMemberRoleInOrganisation should be the last entry in Journey");
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteApprovedPersonLater_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteLater.ToString()
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("CheckYourDetails");
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_PartnershipAndIneligible_Renders_Partail_InEligible()
    {   
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            IsInEligibleToBeApprovedPerson = true,
            IsComplianceScheme = true
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var model = viewResult.Model as AddApprovedPersonViewModel;
        model.Should().NotBeNull();
        model.IsInEligibleToBeApprovedPerson.Should().BeTrue();
        model.IsOrganisationAPartnership.Should().BeTrue();
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_PartnershipOnly_Render_Partial_LimitedPartnership()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = true;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            IsInEligibleToBeApprovedPerson = false,
            IsComplianceScheme = false
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var model = viewResult.Model as AddApprovedPersonViewModel;
        model.Should().NotBeNull();
        model.IsInEligibleToBeApprovedPerson.Should().BeFalse();
        model.IsOrganisationAPartnership.Should().BeTrue();
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_NotPartnershipButIneligible_Renders_Partial_AddNotApprovedPerson()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = false;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {   
            IsInEligibleToBeApprovedPerson = true
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var model = viewResult.Model as AddApprovedPersonViewModel;
        model.Should().NotBeNull();
        model.IsInEligibleToBeApprovedPerson.Should().BeTrue();
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_NotPartnershipAndNotIneligible_ReturnsDefaultView()
    {
        // Arrange
        _orgSessionMock.IsOrganisationAPartnership = false;
        _orgSessionMock.ReExCompaniesHouseSession = new ReExCompaniesHouseSession
        {
            IsInEligibleToBeApprovedPerson = false
        };

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.ViewName.Should().BeNull(); // default view
    }

    [TestMethod]
    public async Task AddApprovedPerson_ModelInvalid_IsPartnership_Renders_Partial_LimitedPartnershipAddApprovedPerson()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();

        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsInEligibleToBeApprovedPerson = false
            }
        };

        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var modelResult = viewResult.Model as AddApprovedPersonViewModel;
        modelResult.Should().NotBeNull();
        modelResult.IsInEligibleToBeApprovedPerson.Should().BeFalse();
    }

    [TestMethod]
    public async Task TeamMemberDetails_IdIsEmpty_RedirectsToTeamMemberRoleInOrganisation()
    {
        // Act
        var result = await _systemUnderTest.TeamMemberDetails();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task AddApprovedPerson_WhenUserIsChangingDetails_SetsBackLinkToCheckYourDetails()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsUserChangingDetails = true,
            Journey = _orgSessionMock.Journey
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task AddApprovedPerson_ModelInvalid_IsNotPartnership_Renders_AddNotApprovedPerson()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsInEligibleToBeApprovedPerson = true
            }
        };

        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var modelResult = viewResult.Model as AddApprovedPersonViewModel;
        modelResult.Should().NotBeNull();
        modelResult.IsInEligibleToBeApprovedPerson.Should().BeTrue();
    }

    [TestMethod]
    public async Task AddApprovedPerson_ModelInvalid_IsNotPartnership_NotIneligible_Renders_DefaultAddApprovedPerson()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsInEligibleToBeApprovedPerson = false
            }
        };

        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");
        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var modelResult = viewResult.Model as AddApprovedPersonViewModel;
        modelResult.Should().NotBeNull();
        modelResult.IsInEligibleToBeApprovedPerson.Should().BeFalse();
        modelResult.IsOrganisationAPartnership.Should().BeFalse();
    }

    [TestMethod]
    public async Task AddApprovedPerson_UnknownOption_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = "SomeUnknownOption"
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("CheckYourDetails");
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_SetsAllModelPropertiesCorrectly()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsInEligibleToBeApprovedPerson = true,
                Partnership = new ReExPartnership
                {
                    IsLimitedPartnership = true,
                    IsLimitedLiabilityPartnership = false
                }
            }
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        var model = viewResult.Model as AddApprovedPersonViewModel;
        model.Should().NotBeNull();
        model.IsOrganisationAPartnership.Should().BeTrue();
        model.IsInEligibleToBeApprovedPerson.Should().BeTrue();
        model.IsLimitedPartnership.Should().BeTrue();
        model.IsLimitedLiablePartnership.Should().BeFalse();
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteAnotherPerson_PartnershipIsLimitedLiability_RedirectsToMemberPartnership()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new ReExPartnership
                {
                    IsLimitedLiabilityPartnership = true
                }
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.MemberPartnership));
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_WhenReExCompaniesHouseSessionIsNull_SetsDefaultModelValues()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false,
            ReExCompaniesHouseSession = null // <- this triggers all `?? false` fallback logic
        };

        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        _sessionManagerMock.Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session)).Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as AddApprovedPersonViewModel;
        model.Should().NotBeNull();
        model.IsLimitedPartnership.Should().BeFalse();
        model.IsLimitedLiablePartnership.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(null, true)]
    public async Task AddApprovedPerson_Get_WhenReExManualInputSessionIsNotNull_SetsIsInEligibleToBeApprovedPersonCorrectly(
        bool? sessionIsEligibleToBeApprovedPerson,
        bool expectedViewModelIsIneligibleToBeApprovedPerson)
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false,
            ReExCompaniesHouseSession = null,
            ReExManualInputSession = new ReExManualInputSession
            {
                IsEligibleToBeApprovedPerson = sessionIsEligibleToBeApprovedPerson
            }
        };

        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        _sessionManagerMock.Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session)).Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model as AddApprovedPersonViewModel;
        model.Should().NotBeNull();
        model.IsInEligibleToBeApprovedPerson.Should().Be(expectedViewModelIsIneligibleToBeApprovedPerson);
    }

    [TestMethod]
    public async Task Post_AddApprovedPerson_WhenInviteAnotherPersonAndLLPTrue_RedirectsToMemberPartnership()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new FrontendAccountCreation.Core.Sessions.ReEx.Partnership.ReExPartnership
                {
                    IsLimitedLiabilityPartnership = true
                }
            }
        };

        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.MemberPartnership));
    }

    [TestMethod]
    public async Task Post_AddApprovedPerson_WhenInviteUserOptionIsUnknown_RedirectsToCheckYourDetails()
    {
        var session = new OrganisationSession();
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = "InvalidOption"
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var result = await _systemUnderTest.AddApprovedPerson(model);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));
    }

    [TestMethod]
    public async Task Post_AddApprovedPerson_WhenNonCompanyHouseFlow_RedirectsToManageControlOrganisation()
    {
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = null,
            OrganisationType = OrganisationType.NonCompaniesHouseCompany
        };

        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var result = await _systemUnderTest.AddApprovedPerson(model);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.ManageControlOrganisation));
    }

    [TestMethod]
    public async Task AddApprovedPerson_InviteUserOption_StoredInSessionAsEnum()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteLater.ToString()
        };

        var session = new OrganisationSession();

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        session.InviteUserOption.Should().Be(InviteUserOptions.InviteLater);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.CheckYourDetails));
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_PopulatesInviteUserOptionFromSession()
    {
        // Arrange
        var session = new OrganisationSession
        {
            InviteUserOption = InviteUserOptions.BeAnApprovedPerson
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock
            .Setup(s => s.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<AddApprovedPersonViewModel>().Subject;
        model.InviteUserOption.Should().Be(InviteUserOptions.BeAnApprovedPerson.ToString());
    }

    [TestMethod]
    public async Task AddApprovedPerson_Get_WithFocusIdFromTempData_SetsFocusIdInController()
    {
        // Arrange
        var focusId = Guid.NewGuid();

        var tempDataMock = new Mock<ITempDataDictionary>();
        tempDataMock.Setup(t => t["FocusId"]).Returns(focusId.ToString());

        _systemUnderTest.TempData = tempDataMock.Object;

        // Act
        await _systemUnderTest.AddApprovedPerson();

        // Assert
        _systemUnderTest.GetFocusId().Should().Be(focusId);
    }

    [TestMethod]
    public async Task AddApprovedPerson_Post_InviteAnotherPerson_NotPartnershipLLP_CompanyHouseFlow_RedirectsToTeamMemberRoleInOrganisation()
    {
        // Arrange
        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false, // Not a partnership
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new ReExPartnership
                {
                    IsLimitedLiabilityPartnership = false // Not LLP
                }
            },
            OrganisationType = OrganisationType.CompaniesHouseCompany
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_systemUnderTest.TeamMemberRoleInOrganisation));
    }

    [TestMethod]
    public async Task AddApprovedPerson_Post_WithFocusId_KeepsFocusIdInController()
    {
        var focusId = Guid.NewGuid();
        var tempDataMock = new Mock<ITempDataDictionary>();
        var tempDataStorage = new Dictionary<string, object>();

        tempDataMock.Setup(t => t[It.IsAny<string>()])
            .Returns((string key) => tempDataStorage.TryGetValue(key, out var value) ? value : null);

        tempDataMock.SetupSet(t => t[It.IsAny<string>()] = It.IsAny<object>())
            .Callback((string key, object value) => tempDataStorage[key] = value);

        _systemUnderTest.TempData = tempDataMock.Object;

        _systemUnderTest.SetFocusId(focusId);

        var model = new AddApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteLater.ToString()
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new OrganisationSession());

        await _systemUnderTest.AddApprovedPerson(model);

        _systemUnderTest.GetFocusId().Should().Be(focusId);
    }

    [TestMethod]
    public async Task TeamMemberRoleInOrganisationAddAnother_DeletesFocusIdAndRedirectsToTeamMemberRoleInOrganisation()
    {
        // Arrange
        var session = new OrganisationSession();
        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Mock TempData for DeleteFocusId
        var tempDataMock = new Mock<ITempDataDictionary>();
        tempDataMock.Setup(t => t.Remove("FocusId")).Returns(true); // Simulate successful removal
        _systemUnderTest.TempData = tempDataMock.Object;

        // Act
        var result = await _systemUnderTest.TeamMemberRoleInOrganisationAddAnother();

        // Assert
        _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        tempDataMock.Verify(t => t.Remove("FocusId"), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(ApprovedPersonController.TeamMemberRoleInOrganisation));
        redirectResult.ControllerName.Should().BeNull(); // Assuming it redirects within the same controller
    }

    [TestMethod]
    public async Task YouAreApprovedPerson_Get_ReturnsViewWithCorrectViewModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Partnership = new ReExPartnership
                {
                    IsLimitedLiabilityPartnership = true,
                    IsLimitedPartnership = false
                }
            }
        };
        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Mock TempData for GetFocusId and SetFocusId
        var focusId = Guid.NewGuid();
        var tempDataMock = new Mock<ITempDataDictionary>();
        var tempDataStorage = new Dictionary<string, object>();
        tempDataMock.Setup(t => t[It.IsAny<string>()])
                    .Returns((string key) => tempDataStorage.TryGetValue(key, out var value) ? value : null);
        tempDataMock.SetupSet(t => t[It.IsAny<string>()] = It.IsAny<object>())
                    .Callback((string key, object value) => tempDataStorage[key] = value);

        _systemUnderTest.TempData = tempDataMock.Object;
        _systemUnderTest.SetFocusId(focusId); // Set a focus ID in TempData before the action is called

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        _sessionManagerMock.Verify(s => s.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);

        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<ApprovedPersonViewModel>().Subject;
        model.IsLimitedLiabilityPartnership.Should().BeTrue();
        model.IsLimitedPartnership.Should().BeFalse();
        _systemUnderTest.GetFocusId().Should().Be(focusId); // Verify FocusId is still available
    }

    [TestMethod]
    public async Task YouAreApprovedPerson_Get_NoPartnershipDetails_ReturnsViewWithDefaultViewModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false,
            ReExCompaniesHouseSession = null // No Companies House session
        };
        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<ApprovedPersonViewModel>().Subject;
        model.IsLimitedLiabilityPartnership.Should().BeFalse();
        model.IsLimitedPartnership.Should().BeFalse();
    }

    [TestMethod]
    public async Task YouAreApprovedPerson_Get_PartnershipNoReExCompaniesHouseSession_ReturnsViewWithDefaultViewModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = true,
            ReExCompaniesHouseSession = null
        };
        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var model = viewResult.Model.Should().BeOfType<ApprovedPersonViewModel>().Subject;
        model.IsLimitedLiabilityPartnership.Should().BeFalse();
        model.IsLimitedPartnership.Should().BeFalse();
    }

    [TestMethod]
    public async Task YouAreApprovedPerson_Get_NoFocusId_FocusIdStaysNull()
    {
        // Arrange
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession()
        };
        _sessionManagerMock.Setup(s => s.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Ensure TempData doesn't contain FocusId
        var tempDataMock = new Mock<ITempDataDictionary>();
        tempDataMock.Setup(t => t["FocusId"]).Returns(null); // Explicitly state no FocusId
        _systemUnderTest.TempData = tempDataMock.Object;

        // Act
        await _systemUnderTest.YouAreApprovedPerson();

        // Assert
        _systemUnderTest.GetFocusId().Should().BeNull();
    }

    [DataTestMethod]
    [DataRow(true, false, false, "AddNotApprovedPerson.SoleTrader.ErrorMessage", DisplayName = "SoleTrader error")]
    [DataRow(false, true, true, "AddApprovedPerson.NonUk.IneligibleAP.ErrorMessage", DisplayName = "NonUk and ineligible error")]
    [DataRow(false, true, false, "AddApprovedPerson.NonUk.EligibleAP.ErrorMessage", DisplayName = "NonUk and eligible error")]
    [DataRow(false, false, false, "AddAnApprovedPerson.OptionError", DisplayName = "Default error")]
    public async Task AddApprovedPerson_ModelStateInvalid_SetsCorrectErrorMessage(
        bool isSoleTrader, bool isNonUk, bool isIneligible, string expectedError)
    {
        // Arrange
        var model = new AddApprovedPersonViewModel();
        var session = new OrganisationSession
        {
            IsOrganisationAPartnership = false,
            IsUkMainAddress = !isNonUk,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                IsInEligibleToBeApprovedPerson = isIneligible
            },
            ReExManualInputSession = isSoleTrader
                ? new ReExManualInputSession
                {
                    ProducerType = ProducerType.SoleTrader
                }
                : null
        };

        _sessionManagerMock
            .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _systemUnderTest.ModelState.AddModelError("InviteUserOption", "Required");

        // Act
        var result = await _systemUnderTest.AddApprovedPerson(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var modelState = viewResult.ViewData.ModelState;
        modelState[nameof(AddApprovedPersonViewModel.InviteUserOption)].Errors
            .Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(expectedError);
    }
}
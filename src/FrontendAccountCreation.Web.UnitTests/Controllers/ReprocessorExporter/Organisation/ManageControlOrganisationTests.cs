using FluentAssertions;
using FrontendAccountCreation.Core.Models;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Pages.Organisation;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class ManageControlOrganisationTests : OrganisationPageModelTestBase<ManageControlOrganisation>
{
    private ManageControlOrganisation _manageControlOrganisation;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        OrganisationSession.Journey =
        [
            PagePath.ManageControlOrganisation,
            PagePath.TeamMemberDetails
        ];
        OrganisationSession.ReExCompaniesHouseSession = new ReExCompaniesHouseSession();
        OrganisationSession.IsUserChangingDetails = false;

        _manageControlOrganisation = new ManageControlOrganisation(SessionManagerMock.Object, SharedLocalizerMock.Object, LocalizerMock.Object)
        {
            PageContext = PageContext
        };
    }

    [TestMethod]
    [DataRow(YesNoNotSure.Yes)]
    [DataRow(YesNoNotSure.No)]
    [DataRow(YesNoNotSure.NotSure)]
    public async Task OnGet_SetsSelectedValueCorrectly(YesNoNotSure? yesNoNotSure)
    {
        // Arrange
        OrganisationSession.TheyManageOrControlOrganisation = yesNoNotSure;

        // Act
        await _manageControlOrganisation.OnGet();

        // Assert
        _manageControlOrganisation.SelectedValue.Should().Be(yesNoNotSure.ToString());
    }

    //[TestMethod]
    //[DataRow(YesNoNotSure.Yes)]
    //[DataRow(null)]
    //public async Task Get_ManageControlOrganisation_ReturnsView_WithViewModel_ParameterValue_AsTrue(YesNoNotSure? yesNoNotSure)
    //{
    //    // Arrange
    //    var session = new OrganisationSession
    //    {
    //        TheyManageOrControlOrganisation = yesNoNotSure
    //    };
    //    _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
    //        .ReturnsAsync(session);

    //    var invitePerson = yesNoNotSure == YesNoNotSure.Yes;

    //    // Act
    //    var result = await _systemUnderTest.ManageControlOrganisation(invitePerson);

    //    // Assert
    //    var viewResult = result.Should().BeOfType<ViewResult>().Subject;
    //    var model = viewResult.Model.Should().BeOfType<ManageControlOrganisationViewModel>().Subject;
    //    model.TheyManageOrControlOrganisation.Should().Be(null);
    //}

    //[TestMethod]
    //public async Task Post_ManageControlOrganisation_With_InvalidModel_ReturnsViewWithModel()
    //{
    //    // Arrange
    //    var session = new OrganisationSession();
    //    _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
    //        .ReturnsAsync(session);

    //    var model = new ManageControlOrganisationViewModel();
    //    _systemUnderTest.ModelState.AddModelError("TheyManageOrControlOrganisation", "Required");

    //    // Act
    //    var result = await _systemUnderTest.ManageControlOrganisation(model);

    //    // Assert
    //    var viewResult = result.Should().BeOfType<ViewResult>().Subject;
    //    viewResult.Model.Should().Be(model);
    //}

    //[TestMethod]
    //[DataRow(YesNoNotSure.Yes)]
    //[DataRow(YesNoNotSure.No)]
    //[DataRow(YesNoNotSure.NotSure)]
    //public async Task Post_ManageControlOrganisation_ValidModel_UpdatesSessionAndRedirects(YesNoNotSure yesNoNotSure)
    //{
    //    // Arrange
    //    var session = new OrganisationSession();
    //    _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
    //        .ReturnsAsync(session);

    //    var model = new ManageControlOrganisationViewModel
    //    {
    //        TheyManageOrControlOrganisation = yesNoNotSure
    //    };

    //    // Act
    //    var result = await _systemUnderTest.ManageControlOrganisation(model);

    //    // Assert
    //    var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;

    //    var actionMethod = yesNoNotSure == YesNoNotSure.Yes ? nameof(ApprovedPersonController.NonCompaniesHouseTeamMemberDetails) : nameof(ApprovedPersonController.PersonCanNotBeInvited);
    //    redirectResult.ActionName.Should().Be(actionMethod);
    //    session.TheyManageOrControlOrganisation.Should().Be(yesNoNotSure);
    //}
}
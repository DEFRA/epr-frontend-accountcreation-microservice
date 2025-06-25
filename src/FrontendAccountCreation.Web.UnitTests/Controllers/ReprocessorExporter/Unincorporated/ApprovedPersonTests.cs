using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Unincorporated;

[TestClass]
public class ApprovedPersonTests : UnincorporatedTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task ApprovedPerson_Get_ReturnsPage()
    {
        // Arrange
        SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.ApprovedPerson();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.UnincorporatedManageAccountPerson);
    }

    [TestMethod]
    public async Task ApprovedPerson_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        SetupOrganisationSession();
        _systemUnderTest.ModelState.AddModelError("inviteApprovedPerson", "Required");

        // Act
        var result = await _systemUnderTest.ApprovedPerson();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, PagePath.UnincorporatedManageAccountPerson);
    }

    [TestMethod]
    public async Task ApprovedPerson_Post_ValidModel_ContinuePressed_RedirectsToCheckYourDetails()
    {
        // Arrange
        var session = SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.ApprovedPerson(false);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(UnincorporatedController.ManageAccountPerson), redirectResult.ActionName);
        _sessionManagerMock.Verify(sm => sm.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once());
    }
    
    [TestMethod]
    public async Task ApprovedPerson_Post_ValidModel_InvitePressed_RedirectsToInviteApprovedUser()
    {
        // Arrange
        var session = SetupOrganisationSession();

        // Act
        var result = await _systemUnderTest.ApprovedPerson(true);

        // Assert
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(UnincorporatedController.ManageAccountPerson), redirectResult.ActionName);
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
                PagePath.UnincorporatedApprovedPerson
            ],
            ReExUnincorporatedFlowSession = new ReExUnincorporatedFlowSession()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        return session;
    }
}
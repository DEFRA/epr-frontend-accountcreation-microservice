using FluentAssertions;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.ApprovedPerson;

[TestClass]
public class NonCompaniesHousePartnershipInviteTest : ApprovedPersonTestBase
{
    private OrganisationSession _orgSessionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _orgSessionMock = new OrganisationSession
        {
            Journey =
            [
                PagePath.NonCompaniesHousePartnershipAddApprovedPerson
            ],
            ReExManualInputSession = new ReExManualInputSession(),
            IsUserChangingDetails = false
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_orgSessionMock);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipInviteApprovedPerson_Get_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var session = new OrganisationSession
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson,
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.Partnership
            }
        };

        _sessionManagerMock
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipInviteApprovedPerson();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<NonCompaniesHousePartnershipInviteApprovedPersonViewModel>();
        var model = (NonCompaniesHousePartnershipInviteApprovedPersonViewModel)viewResult.Model;

        model.InviteUserOption.Should().Be(session.InviteUserOption.ToString());
        model.IsNonCompaniesHousePartnership.Should().BeTrue();

        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipInviteApprovedPerson_Post_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var model = new NonCompaniesHousePartnershipInviteApprovedPersonViewModel
        {
            InviteUserOption = "InvalidValue"
        };

        var session = new OrganisationSession
        {
            ReExManualInputSession = new ReExManualInputSession
            {
                ProducerType = ProducerType.Partnership
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _systemUnderTest.ModelState.AddModelError("SomeField", "Some error");

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipInviteApprovedPerson(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        var returnedModel = viewResult.Model as NonCompaniesHousePartnershipInviteApprovedPersonViewModel;

        returnedModel.Should().NotBeNull();
        returnedModel.IsNonCompaniesHousePartnership.Should().BeTrue();
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipInviteApprovedPerson_Post_InviteAnotherPerson_RedirectsToRolePage()
    {
        // Arrange
        var model = new NonCompaniesHousePartnershipInviteApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteAnotherPerson.ToString()
        };

        var session = new OrganisationSession();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipInviteApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(LimitedPartnershipController.WhatRoleDoTheyHaveWithinThePartnership));
    }

    [TestMethod]
    public async Task NonCompaniesHousePartnershipInviteApprovedPerson_Post_CheckDetails_RedirectsToCheckYourDetails()
    {
        // Arrange
        var model = new NonCompaniesHousePartnershipInviteApprovedPersonViewModel
        {
            InviteUserOption = InviteUserOptions.InviteLater.ToString(),
        };

        var session = new OrganisationSession();

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _sessionManagerMock.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), session))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.NonCompaniesHousePartnershipInviteApprovedPerson(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be(nameof(ApprovedPersonController.CheckYourDetails));
        redirect.ControllerName.Should().Be("ApprovedPerson");
    }
}


using FluentAssertions;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.AccountCreation;

[TestClass]
public class DeclarationTests : AccountCreationTestBase
{
    private AccountCreationSession _accountCreationSessionMock = null!;
    private const string ReportDataLandingRedirectUrl = "/report-data/landing";
    private const string ReportDataRedirectUrl = "/report-data";

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        _accountCreationSessionMock = new AccountCreationSession
        {
            Journey = new List<string> { PagePath.CheckYourDetails, PagePath.Declaration },
        };

        _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
        {
            new("emails", "abc@efg.com")
        }.AsEnumerable());

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_accountCreationSessionMock);
    }

    [TestMethod]
    [DataRow(false, ReportDataLandingRedirectUrl)]
    [DataRow(true, ReportDataRedirectUrl)]
    public async Task Confirm_WhenCalled_ThenRedirectsToReportPackagingDataUrlWhenOrganisationIsComplianceScheme(bool isComplianceScheme, string expectedRedirectUrl)
    {
        _accountServiceMock
            .Setup(x => x.CreateAccountModel(It.IsAny<AccountCreationSession>(), It.IsAny<string>()))
            .Returns(new AccountModel { Organisation = new OrganisationModel { IsComplianceScheme = isComplianceScheme } });

        var result = await _systemUnderTest.Confirm();

        result.Should().BeOfType<RedirectResult>();

        result.As<RedirectResult>().Url.Should().Be(expectedRedirectUrl);
    }

    [TestMethod]
    public async Task Declaration_WhenCalled_ThenDeclarationPageReturnedWithCheckYourDetailsAsTheBackLink()
    {
        var result = await _systemUnderTest.Declaration();

        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeNull();

        AssertBackLink(viewResult, PagePath.CheckYourDetails);
    }

    [TestMethod]
    public async Task Confirm_WhenCalled_PostToAccountCreationEndpointInFacade()
    {
        _accountServiceMock
            .Setup(x => x.CreateAccountModel(It.IsAny<AccountCreationSession>(), It.IsAny<string>()))
            .Returns(new AccountModel { Organisation = new OrganisationModel { IsComplianceScheme = true } });
        
        var result = await _systemUnderTest.Confirm();

        result.Should().BeOfType<RedirectResult>();

        ((RedirectResult)result).Url.Should().Be(ReportDataRedirectUrl);

        _facadeServiceMock.Verify(x => x.PostAccountDetailsAsync(It.IsAny<AccountModel>()), Times.Once);
    }

}
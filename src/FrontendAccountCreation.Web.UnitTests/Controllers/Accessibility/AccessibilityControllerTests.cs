﻿using FluentAssertions;
using FrontendAccountCreation.Web.Configs;
using FrontendAccountCreation.Web.Controllers.Privacy;
using FrontendAccountCreation.Web.ViewModels.Privacy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Accessibility;

[TestClass]
public class AccessibilityControllerTests
{
    private Mock<HttpContext>? _httpContextMock;
    private Mock<HttpRequest>? _httpRequest;

    private Mock<IOptions<ExternalUrlsOptions>> _urlOptions = null!;
    private Mock<IOptions<EmailAddressOptions>> _emailOptions = null!;
    private Mock<IOptions<SiteDateOptions>> _siteDateOptions = null!;

    private AccessibilityController _systemUnderTest = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpRequest = new Mock<HttpRequest>();
        _urlOptions = new Mock<IOptions<ExternalUrlsOptions>>();
        _emailOptions = new Mock<IOptions<EmailAddressOptions>>();
        _siteDateOptions = new Mock<IOptions<SiteDateOptions>>();

        SetUpConfigOption();

        _systemUnderTest = new AccessibilityController(
            _urlOptions.Object,
            _emailOptions.Object,
            _siteDateOptions.Object);

        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequest.Object);
    }

    [TestMethod]
    public void Detail_WhenCalled_ItShouldSetViewModel()
    {
        // Arrange
        const string returnUrl = "~/home/index";
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
            .Returns(true)
            .Verifiable();
        _systemUnderTest.Url = mockUrlHelper.Object;

        var result = _systemUnderTest.Detail(returnUrl);
        var viewResult = (ViewResult)result;
        var accessibilityModel = (AccessibilityViewModel)viewResult.Model!;
        var expectedSiteTestedDate = _siteDateOptions.Object.Value.AccessibilitySiteTested.ToString(_siteDateOptions.Object.Value.DateFormat);
        var expectedStatementPreparedDate = _siteDateOptions.Object.Value.AccessibilityStatementPrepared.ToString(_siteDateOptions.Object.Value.DateFormat);
        var expectedStatementReviewedDate = _siteDateOptions.Object.Value.AccessibilityStatementReviewed.ToString(_siteDateOptions.Object.Value.DateFormat);

        result.Should().BeOfType(typeof(ViewResult));
        accessibilityModel.SiteTestedDate.Should().Be(expectedSiteTestedDate);
        accessibilityModel.StatementPreparedDate.Should().Be(expectedStatementPreparedDate);
        accessibilityModel.StatementReviewedDate.Should().Be(expectedStatementReviewedDate);

        accessibilityModel.DefraHelplineEmail.Should().Be(_emailOptions.Object.Value.DefraHelpline);

        accessibilityModel.AbilityNetUrl.Should().Be(_urlOptions.Object.Value.AccessibilityAbilityNet);
        accessibilityModel.ContactUsUrl.Should().Be(_urlOptions.Object.Value.AccessibilityContactUs);
        accessibilityModel.WebContentAccessibilityUrl.Should().Be(_urlOptions.Object.Value.AccessibilityWebContentAccessibility);
        accessibilityModel.EqualityAdvisorySupportServiceUrl.Should().Be(_urlOptions.Object.Value.AccessibilityEqualityAdvisorySupportService);
    }
    
    [TestMethod]
    public void Detail_WhenCalledWithNoLocalUrl_ItShouldSetViewModel()
    {
        // Arrange
        const string returnUrl = "~/home/index";
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
            .Returns(false)
            .Verifiable();
        _systemUnderTest.Url = mockUrlHelper.Object;

        var result = _systemUnderTest.Detail(returnUrl);
        var viewResult = (ViewResult)result;
        var accessibilityModel = (AccessibilityViewModel)viewResult.Model!;
        var expectedSiteTestedDate = _siteDateOptions.Object.Value.AccessibilitySiteTested.ToString(_siteDateOptions.Object.Value.DateFormat);
        var expectedStatementPreparedDate = _siteDateOptions.Object.Value.AccessibilityStatementPrepared.ToString(_siteDateOptions.Object.Value.DateFormat);
        var expectedStatementReviewedDate = _siteDateOptions.Object.Value.AccessibilityStatementReviewed.ToString(_siteDateOptions.Object.Value.DateFormat);

        result.Should().BeOfType(typeof(ViewResult));
        accessibilityModel.SiteTestedDate.Should().Be(expectedSiteTestedDate);
        accessibilityModel.StatementPreparedDate.Should().Be(expectedStatementPreparedDate);
        accessibilityModel.StatementReviewedDate.Should().Be(expectedStatementReviewedDate);

        accessibilityModel.DefraHelplineEmail.Should().Be(_emailOptions.Object.Value.DefraHelpline);

        accessibilityModel.AbilityNetUrl.Should().Be(_urlOptions.Object.Value.AccessibilityAbilityNet);
        accessibilityModel.ContactUsUrl.Should().Be(_urlOptions.Object.Value.AccessibilityContactUs);
        accessibilityModel.WebContentAccessibilityUrl.Should().Be(_urlOptions.Object.Value.AccessibilityWebContentAccessibility);
        accessibilityModel.EqualityAdvisorySupportServiceUrl.Should().Be(_urlOptions.Object.Value.AccessibilityEqualityAdvisorySupportService);
    }

    private void SetUpConfigOption()
    {
        var externalUrlsOptions = new ExternalUrlsOptions()
        {
            AccessibilityAbilityNet = "url1",
            AccessibilityContactUs = "url2",
            AccessibilityEqualityAdvisorySupportService = "url3",
            AccessibilityWebContentAccessibility = "url4"
        };

        var emailAddressOptions = new EmailAddressOptions()
        {
            DefraHelpline = "test1@email.com",
        };

        var siteDateOptions = new SiteDateOptions()
        {
            AccessibilitySiteTested = DateTime.Parse("2000-01-01"),
            AccessibilityStatementPrepared = DateTime.Parse("2000-01-02"),
            AccessibilityStatementReviewed = DateTime.Parse("2000-01-03"),
            DateFormat = "d MMMM yyyy"
        };

        _urlOptions!
            .Setup(x => x.Value)
            .Returns(externalUrlsOptions);

        _emailOptions!
            .Setup(x => x.Value)
            .Returns(emailAddressOptions);

        _siteDateOptions!
            .Setup(x => x.Value)
            .Returns(siteDateOptions);
    }
}

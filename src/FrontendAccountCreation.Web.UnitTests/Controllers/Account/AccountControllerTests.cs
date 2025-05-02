using FluentAssertions;
using FrontendAccountCreation.Web.Controllers.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.Account;

[TestClass]
public class AccountControllerTests
{
    private Mock<IOptionsMonitor<MicrosoftIdentityOptions>> _optionsMonitorMock;
    private AccountController _accountController;
    private Mock<HttpContext>? _httpContextMock;
    private Mock<HttpRequest> _httpRequest = null!;

    [TestInitialize]
    public void SetUp()
    {
        _optionsMonitorMock = new Mock<IOptionsMonitor<MicrosoftIdentityOptions>>();
        _httpContextMock = new Mock<HttpContext>();
        _httpRequest = new Mock<HttpRequest>();

        var options = new MicrosoftIdentityOptions { ResetPasswordPolicyId = "SomeResetPasswordPolicyId" };
        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(options);
        _optionsMonitorMock.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequest.Object);
        _accountController = new AccountController();

    }
    
    [TestMethod]
    public void SignIn_IfIsLocalUrl_RedirectsToCorrectUrl()
    {
        //Arrange
        const string returnUrl = "~/home/index";
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
            .Returns(true)
            .Verifiable();
        _accountController.Url = mockUrlHelper.Object;
        
        //Act
        var result = _accountController.SignIn("", returnUrl);
        
        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType(typeof(ChallengeResult));

    }
    
    [TestMethod]
    public void SignIn_IfIsNotLocalUrl_RedirectsToCorrectUrl()
    {
        //Arrange
        const string returnUrl = "~/home/index";
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
            .Returns(false)
            .Verifiable();
        _accountController.Url = mockUrlHelper.Object;
        
        //Act
        var result = _accountController.SignIn("", returnUrl);
        
        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType(typeof(ChallengeResult));

    }
    
    [TestMethod]
    [DataRow(null, "redirectUri")]
    [DataRow(false, "redirectUri")]
    [DataRow(true, "redirectUri?reEx=true")]
    public void SignOut_RedirectsToCorrectUrl(bool? reEx, string expectedRedirectUri)
    {
        //Arrange
        var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);

        mockUrlHelper
            .Setup(
                x => x.Action(
                    It.IsAny<UrlActionContext>()
                )
            )
            .Returns<UrlActionContext>(context =>
            {
                var reExValue = context.Values?.GetType().GetProperty("reEx")?.GetValue(context.Values, null) as bool?;

                return reExValue switch
                {
                    true => "redirectUri?reEx=true",
                    _ => "redirectUri"
                };
            })
            .Verifiable();

        _accountController.Url = mockUrlHelper.Object;
        _accountController.ControllerContext.HttpContext = new DefaultHttpContext();
        
        //Act
        var result = _accountController.SignOut("Scheme", reEx);
        
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType(typeof(SignOutResult));
        var signOutResult = result as SignOutResult;
        signOutResult!.Properties.Should().NotBeNull();
        signOutResult!.Properties!.RedirectUri.Should().Be(expectedRedirectUri);
    }
}
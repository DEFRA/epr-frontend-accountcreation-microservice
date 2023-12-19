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
        _accountController = new AccountController(_optionsMonitorMock.Object);

    }
    
    [TestMethod]
    public void SigIn_IfIsLocalUrl_RedirectsToCorrectUrl()
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
    public void SigIn_IfIsNotLocalUrl_RedirectsToCorrectUrl()
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
    public void SigOut_RedirectsToCorrectUrl()
    {
        //Arrange
        var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
        mockUrlHelper
            .Setup(
                x => x.Action(
                    It.IsAny<UrlActionContext>()
                )
            )
            .Returns("callbackUrl")
            .Verifiable();
        _accountController.Url = mockUrlHelper.Object;
        _accountController.ControllerContext.HttpContext = new DefaultHttpContext();
       
        
        //Act
        var result = _accountController.SignOut("Scheme");
        
        //Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType(typeof(SignOutResult));

    }
}
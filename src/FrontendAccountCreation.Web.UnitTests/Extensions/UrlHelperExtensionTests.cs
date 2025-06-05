namespace FrontendAccountCreation.Web.UnitTests.Extensions;

using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FrontendAccountCreation.Web.Extensions;
using Microsoft.AspNetCore.Mvc.Routing;

[TestClass]
public class UrlHelperExtensionTests
{
    [TestMethod]
    public void HomePath_ReturnsCorrectPath()
    {
        // Arrange
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.Is<UrlActionContext>(uac => uac.Action == "RegisteredAsCharity" && uac.Controller == "AccountCreation")))
                     .Returns("/AccountCreation/RegisteredAsCharity");

        // Act
        var result = mockUrlHelper.Object.HomePath();

        // Assert
        result.Should().Be("/AccountCreation/RegisteredAsCharity");
    }

    [TestMethod]
    public void CurrentPath_ReturnsCorrectPath()
    {
        // Arrange
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.Is<UrlActionContext>(uac => uac.Action == null && uac.Controller == "AccountCreation")))
                     .Returns("/AccountCreation");

        // Act
        var result = mockUrlHelper.Object.CurrentPath();

        // Assert
        result.Should().Be("/AccountCreation");
    }

    [TestMethod]
    public void HomePathReExUser_ReturnsCorrectPath()
    {
        // Arrange
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.Is<UrlActionContext>(uac => uac.Action == "ReExAccountFullName" && uac.Controller == "User")))
                     .Returns("/User/ReExAccountFullName");

        // Act
        var result = mockUrlHelper.Object.HomePathReExUser();

        // Assert
        result.Should().Be("/User/ReExAccountFullName");
    }

    [TestMethod]
    public void HomePathReExOrganisation_ReturnsCorrectPath()
    {
        // Arrange
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.Is<UrlActionContext>(uac => uac.Action == "RegisteredAsCharity" && uac.Controller == "Organisation")))
                     .Returns("/Organisation/RegisteredAsCharity");

        // Act
        var result = mockUrlHelper.Object.HomePathReExOrganisation();

        // Assert
        result.Should().Be("/Organisation/RegisteredAsCharity");
    }

    [TestMethod]
    public void CurrentPathReExUser_ReturnsExpectedUrl()
    {
        // Arrange
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.Is<UrlActionContext>(uac => uac.Action == null && uac.Controller == "User")))
                     .Returns("/User");

        // Act
        var result = mockUrlHelper.Object.CurrentPathReExUser();

        // Assert
        Assert.AreEqual("/User", result);
    }

    [TestMethod]
    public void CurrentPathReExOrganisation_ReturnsExpectedUrl()
    {
        // Arrange
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(x => x.Action(It.Is<UrlActionContext>(uac => uac.Action == null && uac.Controller == "Organisation")))
                     .Returns("/Organisation");

        // Act
        var result = mockUrlHelper.Object.CurrentPathReExOrganisation();

        // Assert
        Assert.AreEqual("/Organisation", result);
    }
}

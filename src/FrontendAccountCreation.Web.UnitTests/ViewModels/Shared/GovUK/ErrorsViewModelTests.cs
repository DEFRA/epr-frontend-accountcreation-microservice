using FrontendAccountCreation.Web.ViewModels.Shared.GovUK;
using Microsoft.AspNetCore.Mvc.Localization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendAccountCreation.Web.UnitTests.ViewModels.Shared.GovUK
{
    [TestClass]
    public class ErrorsViewModelTests
    {
        [TestMethod]
        public void Constructor_WithIViewLocalizerAndFieldOrderDictionary_OrdersAndLocalizesErrors()
        {
            // Arrange
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("person[1].name", new List<ErrorViewModel> { new ErrorViewModel { Key = "person[1].name", Message = "NameError" } }),
                ("person[0].age", new List<ErrorViewModel> { new ErrorViewModel { Key = "person[0].age", Message = "AgeError" } }),
                ("person[0].name", new List<ErrorViewModel> { new ErrorViewModel { Key = "person[0].name", Message = "NameError" } }),
                ("address", new List<ErrorViewModel> { new ErrorViewModel { Key = "address", Message = "AddressError" } }),
            };

            var fieldOrder = new Dictionary<string, string[]>
            {
                { "address", Array.Empty<string>() },
                { "person", new[] { "name", "age" } }
            };

            var localizerMock = new Mock<IViewLocalizer>();
            localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string s) => new LocalizedHtmlString(s, $"Localized_{s}"));

            // Act
            var viewModel = new ErrorsViewModel(errors, localizerMock.Object, fieldOrder);

            // Assert
            Assert.AreEqual(4, viewModel.Errors.Count);
            Assert.AreEqual("address", viewModel.Errors[0].Key);
            Assert.AreEqual("Localized_AddressError", viewModel.Errors[0].Errors[0].Message);
            Assert.AreEqual("person[0].name", viewModel.Errors[1].Key);
            Assert.AreEqual("Localized_NameError", viewModel.Errors[1].Errors[0].Message);
            Assert.AreEqual("person[0].age", viewModel.Errors[2].Key);
            Assert.AreEqual("Localized_AgeError", viewModel.Errors[2].Errors[0].Message);
            Assert.AreEqual("person[1].name", viewModel.Errors[3].Key);
            Assert.AreEqual("Localized_NameError", viewModel.Errors[3].Errors[0].Message);
        }
    }
}

using FluentAssertions;
using FrontendAccountCreation.Web.ViewModels.ReExAccount;

namespace FrontendAccountCreation.Web.UnitTests.ViewModels;

[TestClass]
public class LimitedPartnershipPersonOrCompanyViewModelTests
{
    [TestMethod]
    [DataRow(null, false)]
    [DataRow("", false)]
    [DataRow("     ", false)]
    [DataRow("Joanne Smith", true)]
    public void IsPerson_Returns_CorrectState(string personName, bool expectedValue)
    {
        LimitedPartnershipPersonOrCompanyViewModel model = new LimitedPartnershipPersonOrCompanyViewModel
        {
            PersonName = personName
        };

        model.IsPerson.Should().Be(expectedValue);
    }

    [TestMethod]
    [DataRow(null, false)]
    [DataRow("", false)]
    [DataRow("     ", false)]
    [DataRow("Biffa Waste Inc", true)]
    public void IsCompany_Returns_CorrectState(string companyName, bool expectedValue)
    {
        LimitedPartnershipPersonOrCompanyViewModel model = new LimitedPartnershipPersonOrCompanyViewModel
        {
            CompanyName = companyName
        };

        model.IsCompany.Should().Be(expectedValue);
    }

    [TestMethod]
    [DataRow(null, null, false)]
    [DataRow(null, "", false)]
    [DataRow("", null, false)]
    [DataRow("     ", "     ", false)]
    [DataRow("Joanne Smith", "", true)]
    [DataRow("  ", "Biffa Waste Inc", true)]
    [DataRow("Joanne Smith", "Biffa Waste Inc", false)]
    public void IsPersonOrCompanyButNotBoth_Returns_CorrectState(string personName, string companyName, bool expectedValue)
    {
        LimitedPartnershipPersonOrCompanyViewModel model = new LimitedPartnershipPersonOrCompanyViewModel
        {
            PersonName = personName,
            CompanyName = companyName
        };

        model.IsPersonOrCompanyButNotBoth.Should().Be(expectedValue);
    }

    [TestMethod]
    public void Maps_To_Person_From_ReExLimitedPartnershipPersonOrCompany()
    {
        ReExPersonOrCompanyPartner fromModel = new()
        {
            Id = Guid.NewGuid(),
            IsPerson = true,
            Name = "Joanne Smith"
        };

        LimitedPartnershipPersonOrCompanyViewModel model = (LimitedPartnershipPersonOrCompanyViewModel)fromModel;

        model.Id.Should().Be(fromModel.Id);
        model.PersonName.Should().Be(fromModel.Name);
        model.CompanyName.Should().BeNull();
    }

    [TestMethod]
    public void Maps_To_Company_From_ReExLimitedPartnershipPersonOrCompany()
    {
        ReExPersonOrCompanyPartner fromModel = new()
        {
            Id = Guid.NewGuid(),
            IsPerson = false,
            Name = "Biffa Waste Inc"
        };

        LimitedPartnershipPersonOrCompanyViewModel model = (LimitedPartnershipPersonOrCompanyViewModel)fromModel;

        model.Id.Should().Be(fromModel.Id);
        model.PersonName.Should().BeNull();
        model.CompanyName.Should().Be(fromModel.Name);
    }
}
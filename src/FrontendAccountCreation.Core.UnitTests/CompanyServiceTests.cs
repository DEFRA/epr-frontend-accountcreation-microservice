using FrontendAccountCreation.Core.Services.Dto.Company;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Core.UnitTests;

[TestClass]
public class CompaniesServiceTests
{
    [TestMethod]
    public void WhenValidCompaniesHouseNumber_ThenIsComplianceSchemeIsTrue()
    {
        string companiesHouseNumber = "999999";

        ComplianceSchemeOptions complianceSchemeOptions = new ComplianceSchemeOptions();
        IOptions<ComplianceSchemeOptions> optionParameter = Options.Create(complianceSchemeOptions);

        List<string> chn = new List<string>();
        chn.Add("999999");
        chn.Add("123456");
        complianceSchemeOptions.CompanyHouseNumbers = chn;

        Services.CompanyService cs = new Services.CompanyService(optionParameter);

        Assert.IsTrue(cs.IsComplianceScheme(companiesHouseNumber));

      
    }
    [TestMethod]
    public void WhenValidCompaniesHouseNumber_ThenIsComplianceSchemeIsFalse()
    {
        string companiesHouseNumber = "888888";

        ComplianceSchemeOptions complianceSchemeOptions = new ComplianceSchemeOptions();
        IOptions<ComplianceSchemeOptions> optionParameter = Options.Create(complianceSchemeOptions);

        List<string> chn = new List<string>();
        chn.Add("999999");
        chn.Add("123456");
        complianceSchemeOptions.CompanyHouseNumbers = chn;

        Services.CompanyService cs = new Services.CompanyService(optionParameter);

        Assert.IsFalse(cs.IsComplianceScheme(companiesHouseNumber));


    }
}

using FrontendAccountCreation.Core.Services.Dto.Company;

using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Core.Services;

public class CompanyService : ICompanyService
{
    private readonly HashSet<string> _complianceSchemeOptions;

    public CompanyService(IOptions<ComplianceSchemeOptions> complianceSchemeOptions)
    {
        _complianceSchemeOptions = new HashSet<string>(complianceSchemeOptions.Value.CompanyHouseNumbers);
    }

    public bool IsComplianceScheme(string companiesHouseNumber)
    {
        companiesHouseNumber = companiesHouseNumber.TrimStart('0');
        return _complianceSchemeOptions.Contains(companiesHouseNumber);
    }
}

using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;

namespace FrontendAccountCreation.Core.Services;

using Dto.User;

[ExcludeFromCodeCoverage]
public class MockedFacadeService : IFacadeService
{
    public async Task<string> GetTestMessageAsync()
    {
        return await Task.FromResult("Dummy test message from MockedFacadeService");
    }

    public async Task<Company?> GetCompanyByCompaniesHouseNumberAsync(string companiesHouseNumber)
    {
        return await Task.FromResult(GetDummyCompany(companiesHouseNumber));
    }

    public async Task<AddressList?> GetAddressListByPostcodeAsync(string postcode)
    {
        return await Task.FromResult(GetDummyAddresses(postcode));
    }

    public Task PostAccountDetailsAsync(AccountModel account)
    {
        return Task.CompletedTask;
    }

    public Task PostReprocessorExporterAccountAsync(ReprocessorExporterAccountModel account, string serviceKey)
    {
        return Task.CompletedTask;
    }

    public Task PostEnrolInvitedUserAsync(EnrolInvitedUserModel enrolInvitedUser)
    {
        return Task.CompletedTask;
    }
    
    public Task<UserAccount?> GetUserAccount()
    {
        var userAccountModel = new UserAccount
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EnrolmentStatus = "Invited"
            }
        };
        return Task.FromResult(userAccountModel);
    }
    
    public async Task<bool> DoesAccountAlreadyExistAsync()
    {
        return await Task.FromResult(false);
    }

    private static AddressList GetDummyAddresses(string postcode)
    {
        var addressList = new AddressList
        {
            Addresses = new List<Address>()
            {
                new()
                {
                    AddressSingleLine = "10 Gracefield Gardens, London",
                    BuildingNumber = "10",
                    Street = "Gracefield Gardens",
                    Town = "London",
                    Postcode = postcode,
                },
                new()
                {
                    AddressSingleLine = "11 Gracefield Gardens, London",
                    BuildingNumber = "11",
                    Street = "Gracefield Gardens",
                    Town = "London",
                    Postcode = postcode,
                },
                new()
                {
                    AddressSingleLine = "12 Gracefield Gardens, London",
                    BuildingNumber = "12",
                    Street = "Gracefield Gardens",
                    Town = "London",
                    Postcode = postcode,
                }
            }
        };

        return addressList;
    }

    private static Company GetDummyCompany(string companiesHouseNumber)
    {
        var company = new Company
        {
            CompaniesHouseNumber = "01234567",
            Name = "Dummy Company",
            BusinessAddress = new Address
            {
                BuildingNumber = "10",
                BuildingName = "Dummy Place",
                Street = "Dummy Street",
                Town = "Nowhere",
                Postcode = "AB1 0CD"
            },
            AccountCreatedOn = companiesHouseNumber.Contains('X') ? DateTime.Now : null
        };
        return company;
    }
    public async Task<InviteApprovedUserModel> GetServiceRoleIdAsync(string token)
    {
        return await Task.FromResult(new InviteApprovedUserModel()
        {
            ServiceRoleId = "7",
            CompanyHouseNumber = "0000001",
            Email = "adas@sdad.com"
        });
    }
    
    public async Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteTokenAsync(string token)
    {
        return await Task.FromResult(new ApprovedPersonOrganisationModel());
    }

    public Task PostApprovedUserAccountDetailsAsync(AccountModel account)
    {
        return Task.CompletedTask;
    }
}

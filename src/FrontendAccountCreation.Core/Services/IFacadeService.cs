using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;

namespace FrontendAccountCreation.Core.Services;

using Dto.User;

public interface IFacadeService
{
    Task<string> GetTestMessageAsync();
    Task<Company?> GetCompanyByCompaniesHouseNumberAsync(string companiesHouseNumber);
    Task<AddressList?> GetAddressListByPostcodeAsync(string postcode);
    Task<bool> DoesAccountAlreadyExistAsync();
    Task PostAccountDetailsAsync(AccountModel account);
    Task PostEnrolInvitedUserAsync(EnrolInvitedUserModel enrolInvitedUser);
    Task<UserAccount?> GetUserAccount();
}

using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;
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
    Task PostReprocessorExporterAccountAsync(ReprocessorExporterAccountModel account, string serviceKey);
    Task PostReprocessorExporterCreateOrganisationAsync(ReExOrganisationModel reExOrganisation, string serviceKey);
    Task PostEnrolInvitedUserAsync(EnrolInvitedUserModel enrolInvitedUser);
    Task<UserAccount?> GetUserAccount();
    Task<InviteApprovedUserModel> GetServiceRoleIdAsync(string token);
    Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteTokenAsync(string token);
    Task PostApprovedUserAccountDetailsAsync(AccountModel account);
}
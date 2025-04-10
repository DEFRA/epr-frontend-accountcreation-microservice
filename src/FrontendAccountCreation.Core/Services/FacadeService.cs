using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Exceptions;
using FrontendAccountCreation.Core.Services.Dto.Address;
using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static Microsoft.Identity.Web.Constants;

namespace FrontendAccountCreation.Core.Services;

using Dto.User;

public class FacadeService : IFacadeService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string _baseAddress;
    private readonly string[] _scopes;

    public FacadeService(HttpClient httpClient, ITokenAcquisition tokenAcquisition, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _baseAddress = configuration["FacadeAPI:Address"];
        _scopes = new[]
        {
            configuration["FacadeAPI:DownStreamScope"],
        };
    }

    public async Task<string> GetTestMessageAsync()
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync("/api/test");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            return response.ToString();
        }
    }

    public async Task<Company?> GetCompanyByCompaniesHouseNumberAsync(string companiesHouseNumber)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"/api/companies-house?id={companiesHouseNumber}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var company = await response.Content.ReadFromJsonAsync<CompaniesHouseCompany>();

        return new Company(company);
    }

    public async Task<AddressList?> GetAddressListByPostcodeAsync(string postcode)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"/api/address-lookup?postcode={postcode}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var addressResponse = await response.Content.ReadFromJsonAsync<AddressLookupResponse>();

        return new AddressList(addressResponse);
    }

    public async Task PostAccountDetailsAsync(AccountModel account)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.PostAsJsonAsync("/api/producer-accounts", account);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }

        response.EnsureSuccessStatusCode();
    }

    // possible todo: could have a generic method to do the heavy lifting and handle deserialization better
    public async Task PostReprocessorExporterAccountAsync(ReprocessorExporterAccountModel account)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/reprocessor-exporter-accounts", account);

        if (!response.IsSuccessStatusCode)
        {
            ProblemDetails? problemDetails = null;
            try
            {
                problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            }
            catch (JsonException e)
            {
                // if the response isn't a valid ProblemDetails, either this exception is thrown,
                // or in some circumstances, null is returned.
                // we handle both scenarios next
            }

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();
        }
    }

    public async Task PostEnrolInvitedUserAsync(EnrolInvitedUserModel enrolInvitedUser)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.PostAsJsonAsync("/api/accounts-management/enrol-invited-user", enrolInvitedUser);

        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> DoesAccountAlreadyExistAsync()
    {
        await PrepareAuthenticatedClient();
        var response = await _httpClient.GetAsync($"/api/persons/current");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return false;
        }
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<UserAccount?> GetUserAccount()
    {
        await PrepareAuthenticatedClient();
        var response = await _httpClient.GetAsync("/api/user-accounts");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserAccount>();
    }

    private async Task PrepareAuthenticatedClient()
    {
        _httpClient.BaseAddress ??= new Uri(_baseAddress);
        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Bearer, accessToken);
    }

    public async Task<InviteApprovedUserModel> GetServiceRoleIdAsync(string token)
    {
        await PrepareAuthenticatedClient();
        var response = await _httpClient.GetAsync($"/api/persons/person-by-invite-token?token={token}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var inviteApprovedUser = await response.Content.ReadFromJsonAsync<InviteApprovedUserModel>();

        return inviteApprovedUser;
    }

    public async Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteTokenAsync(string token)
    {
        await PrepareAuthenticatedClient();

        var response = await _httpClient.GetAsync($"/api/organisations/organisation-name?token={token}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var organisation = await response.Content.ReadFromJsonAsync<ApprovedPersonOrganisationModel>();

        return organisation;
    }

    public async Task PostApprovedUserAccountDetailsAsync(AccountModel account)
    {
        await PrepareAuthenticatedClient();
        var response = await _httpClient.PostAsJsonAsync("/api/producer-accounts/ApprovedUser", account);

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails != null)
            {
                throw new ProblemResponseException(problemDetails, response.StatusCode);
            }
        }

        response.EnsureSuccessStatusCode();
    }
}

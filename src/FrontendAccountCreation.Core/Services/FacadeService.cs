using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.Dto.Address;
using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;
using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Services.FacadeModels;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FrontendAccountCreation.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
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
        _httpClient.BaseAddress = new Uri(_baseAddress);
        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Bearer, accessToken);
    }
}

namespace FrontendAccountCreation.Core.UnitTests;

using System.Net;
using System.Text.Json;
using Addresses;
using Constants;
using Exceptions;
using FluentAssertions;
using Services;
using Services.Dto.Address;
using Services.Dto.CompaniesHouse;
using Services.FacadeModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Services.Dto.User;

[TestClass]
public class FacadeServiceTests
{
    private Mock<HttpMessageHandler> _mockHandler = null!;
    private Mock<ITokenAcquisition> _tokenAcquisitionMock = null!;
    private HttpClient _httpClient = null!;
    private FacadeService _facadeService = null!;
    private IConfiguration _configuration;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        _tokenAcquisitionMock = new Mock<ITokenAcquisition>();
        _httpClient = new HttpClient(_mockHandler.Object);
        _httpClient.BaseAddress = new Uri("http://example");

        var inMemorySettings = new Dictionary<string, string>
        {
            { "FacadeAPI:Address", "http://example/" },
            {
                "FacadeAPI:DownStreamScope",
                "https://eprb2cdev.onmicrosoft.com/account-creation-facade/account-creation"
            }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _configuration);
    }

    [TestMethod]
    public async Task GetTestMessageAsyncTest_WithSuccessfulResponseCode()
    {
        // Arrange
        var expectedResult = "Test";
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(expectedResult)
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.GetTestMessageAsync();

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(expected: expectedResult, actual: response);
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task GetTestMessageAsyncTest_WithUnsuccessfulResponseCode()
    {
        // Arrange
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.GetTestMessageAsync();

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Contains("StatusCode: 500"));
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task GetCompanyByCompanyHouseNumber_WhenNoContent_ReturnsNull()
    {
        // Arrange
        var companyHouseNumber = "001";
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NoContent
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.GetCompanyByCompaniesHouseNumberAsync(companyHouseNumber);

        // Assert
        Assert.IsNull(response);
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task GetCompanyByCompanyHouseNumber_ReturnsCompanyObject()
    {
        // Arrange
        var companyHouseNumber = "001";
        var expectedResponse = new CompaniesHouseCompany
        {
            AccountExists = true,
            Organisation = new Organisation
            {
                Name = "Test Org",
                RegisteredOffice = new RegisteredOfficeAddress
                {
                    Postcode = "BT11 8NR",
                    Street = "Main Street",
                    IsUkAddress = true
                },
            },
            AccountCreatedOn = DateTimeOffset.Now
        };

        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.GetCompanyByCompaniesHouseNumberAsync(companyHouseNumber);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(expected: expectedResponse.Organisation.Name, actual: response.Name);
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task GetAddressListByPostcode_WithNoContent_ReturnsNull()
    {
        // Arrange
        var postCode = "001";

        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NoContent
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.GetAddressListByPostcodeAsync(postCode);

        // Assert
        Assert.IsNull(response);
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task GetAddressListByPostcode_AddressList_WithOkResponseCode()
    {
        // Arrange
        var postCode = "001";
        var expectedResponse = new AddressLookupResponse
        {
            Results = Array.Empty<AddressLookupResponseResult>(),
            Header = new AddressLookupResponseHeader()
        };

        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.GetAddressListByPostcodeAsync(postCode);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<AddressList>(response);
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task PostAccountDetails_WithValidData_WithOkResponseCode()
    {
        // Arrange
        var account = new AccountModel
        {
            Person = new PersonModel
            {
                FirstName = "Sherlock",
                LastName = "Holmes",
                ContactEmail = "sherlock.holmes@test.com",
                TelephoneNumber = "074440221"
            },
            Connection = new ConnectionModel
            {
                JobTitle = "Detective",
                ServiceRole = "Test"
            },
            Organisation = new OrganisationModel
            {
                CompaniesHouseNumber = null,
                Address = new AddressModel(),
                IsComplianceScheme = false,
                Name = "Test",
                ValidatedWithCompaniesHouse = true
            }
        };

        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Created
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        await _facadeService.PostAccountDetailsAsync(account);

        // Assert
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri != null &&
                req.Method == HttpMethod.Post
            ),
            ItExpr.IsAny<CancellationToken>()
        );
        httpTestHandler.Dispose();
    }

    [TestMethod]
    [ExpectedException(typeof(ProblemResponseException))]
    public async Task PostAccountDetails_WithUnsuccessfulCode_ThrowsExceptionWithProblemDetails()
    {
        // Arrange
        var account = new AccountModel
        {
            Person = new PersonModel
            {
                FirstName = "Sherlock",
                LastName = "Holmes",
                ContactEmail = "sherlock.holmes@test.com",
                TelephoneNumber = "074440221"
            },
            Connection = new ConnectionModel
            {
                JobTitle = "Detective",
                ServiceRole = "Test"
            },
            Organisation = new OrganisationModel
            {
                CompaniesHouseNumber = null,
                Address = new AddressModel(),
                IsComplianceScheme = false,
                Name = "Test",
                ValidatedWithCompaniesHouse = true
            }
        };

        var problemDetails = new ProblemDetails
        {
            Status = 404,
            Detail = "Unit Test"
        };

        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent(JsonSerializer.Serialize(problemDetails))
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        await _facadeService.PostAccountDetailsAsync(account);
        
        // Assert
        Assert.IsTrue(true);
        
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task PostEnrolInvitedUser_WithValidData_ReturnsSuccessfulCode()
    {
        // Arrange
        var enrolInvitedUser = new EnrolInvitedUserModel
        {
            FirstName = "Sherlock",
            InviteToken = "InviteToken",
            LastName = "Holmes"
        };

        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Created
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        await _facadeService.PostEnrolInvitedUserAsync(enrolInvitedUser);

        // Assert
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri != null &&
                req.Method == HttpMethod.Post
            ),
            ItExpr.IsAny<CancellationToken>()
        );

        httpTestHandler.Dispose();
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task PostEnrolInvitedUser_WithValidData_ReturnsUnsuccessfulCode()
    {
        // Arrange
        var enrolInvitedUser = new EnrolInvitedUserModel
        {
            FirstName = "Sherlock",
            InviteToken = "InviteToken",
            LastName = "Holmes"
        };

        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        await _facadeService.PostEnrolInvitedUserAsync(enrolInvitedUser);
        
        // Assert
        Assert.IsTrue(true);
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task DoesAccountAlreadyExist_UnsuccessfulCode()
    {
        // Arrange
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NoContent
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.DoesAccountAlreadyExistAsync();

        // Assert
        Assert.AreEqual(expected: false, actual: response);
        httpTestHandler.Dispose();
    }

    [TestMethod]
    public async Task DoesAccountAlreadyExist_SuccessfulCode()
    {
        // Assert
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);

        // Act
        var response = await _facadeService.DoesAccountAlreadyExistAsync();

        // Assert
        Assert.AreEqual(expected: true, actual: response);
        httpTestHandler.Dispose();
    }
    
    [TestMethod]
    public async Task GetUserAccount_WhenStatusCodeIsOk_ReturnsUserAccountModel()
    {
        // Arrange
        var userAccountModel = new UserAccount
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EnrolmentStatus = EnrolmentStatus.Enrolled
            }
        };
        
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(userAccountModel))
        };
        
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);
        
        // Act
        var response = await _facadeService.GetUserAccount();
        
        // Assert
        response.Should().BeOfType<UserAccount>().And.BeEquivalentTo(userAccountModel);
    }
    
    [TestMethod]
    public async Task GetUserAccount_WhenStatusCodeIsNotFound_ReturnsDefault()
    {
        // Arrange
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };
        
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);
        
        // Act
        var response = await _facadeService.GetUserAccount();
        
        // Assert
        response.Should().Be(default);
    }
    
    [TestMethod]
    public async Task GetUserAccount_WhenStatusCodeIsUnsuccessful_ThrowsException()
    {
        // Arrange
        var httpTestHandler = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };
        
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpTestHandler);
        
        // Act / Assert
        await _facadeService.Invoking(x => x.GetUserAccount())
            .Should()
            .ThrowAsync<HttpRequestException>();
    }
}
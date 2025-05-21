using FluentAssertions;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Constants;
using FrontendAccountCreation.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.Organisation;

[TestClass]
public class DeclarationTests : OrganisationTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task Get_Declaration_Returns_View()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.Declaration();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task DeclarationContinue_Returns_HappyPath_Calling_FacadeService_PostReprocessorExporterCreateOrganisationAsync()
    {
        //Arrange 
        var orgSession = new OrganisationSession
        {
            Journey = [PagePath.Declaration, PagePath.DeclarationContinue],
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ReExCompaniesHouseSession = new ReExCompaniesHouseSession
            {
                Company = new Core.Services.Dto.Company.Company
                {
                    AccountCreatedOn = DateTime.Now,
                    Name = "ReEx Test Ltd",
                    CompaniesHouseNumber = "12345678",
                    OrganisationId = "06352abc-bb77-4855-9705-cf06ae88f5a8",
                    BusinessAddress = new Address
                    {
                        BuildingName = "ReEx House",
                        BuildingNumber = "14",
                        Street = "High street",
                        Town = "Lodnon",
                        Postcode = "E10 6PN",
                        Locality = "XYZ",
                        DependentLocality = "ABC",
                        County = "London",
                        Country = "England"
                    }
                }
            },
            UkNation = Nation.England
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(orgSession)
            .Verifiable();

        var mapperObj = new ReExOrganisationModel
        {
            CompanyName = "ReEx Test Ltd",
            CompaniesHouseNumber = "12345678",
            OrganisationId = "06352abc-bb77-4855-9705-cf06ae88f5a8",
            CompanyAddress = new AddressModel
            {
                BuildingName = "ReEx House",
                BuildingNumber = "14",
                Street = "High street",
                Town = "Lodnon",
                Postcode = "E10 6PN",
                Locality = "XYZ",
                DependentLocality = "ABC",
                County = "London",
                Country = "England"
            },
            Nation = Nation.England,
            OrganisationType = OrganisationType.CompaniesHouseCompany,
            ValidatedWithCompaniesHouse = true
        };

        _reExAccountMapperMock.Setup(x => x.CreateReExOrganisationModel(orgSession))
            .Returns(mapperObj);

        //Act
        var result = _systemUnderTest.DeclarationContinue();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskStatus.RanToCompletion);
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        _facadeServiceMock.Verify(f => f.PostReprocessorExporterCreateOrganisationAsync(
            It.Is<ReExOrganisationModel>(x =>
                x.OrganisationType == OrganisationType.CompaniesHouseCompany
                && x.CompaniesHouseNumber == "12345678"
                && x.CompanyName == "ReEx Test Ltd"
                && x.Nation == Nation.England), "Re-Ex"), Times.Once);
    }
}

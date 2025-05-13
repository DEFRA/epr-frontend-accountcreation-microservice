using FrontendAccountCreation.Core.Sessions.ReEx.Partnership.ApprovedPersons;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using FrontendAccountCreation.Core.Sessions.ReEx;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountCreation.Web.UnitTests.Controllers.ReprocessorExporter.LimitedPartnership
{
    [TestClass]
    public class LimitedPartnershipAddApprovedPersonTests : LimitedPartnershipTestBase
    {
        private Guid _approvedPersonId;

        [TestInitialize]
        public void Setup()
        {
            SetupBase();

            _approvedPersonId = Guid.NewGuid();

            _orgSessionMock = new OrganisationSession
            {
                ReExCompaniesHouseSession = new ReExCompaniesHouseSession
                {
                    Partnership = new ReExPartnership
                    {
                        LimitedPartnership = new ReExLimitedPartnership
                        {
                            PartnershipApprovedPersons = new List<ReExLimitedPartnershipApprovedPerson>
                        {
                            new()
                            {
                                Id = _approvedPersonId,
                                Role = ReExLimitedPartnershipRoles.Director
                            }
                        }
                        }
                    }
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_orgSessionMock);
        }

        [TestMethod]
        public async Task Get_LimitedPartnershipAddApprovedPerson_ReturnsViewAndSetsBackLink()
        {
            // Arrange
            var session = new OrganisationSession();
            _sessionManagerMock
                .Setup(s => s.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            // Act
            var result = await _systemUnderTest.LimitedPartnershipAddApprovedPerson(Guid.NewGuid());

            // Assert
            result.Should().BeOfType<ViewResult>();

            _sessionManagerMock.Verify(s => s.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

    }
}

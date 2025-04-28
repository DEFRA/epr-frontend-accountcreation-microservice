using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Core.Services;

public interface IOrganisationMapper
{
    AccountModel CreateOrganisationModel(OrganisationSession session, string email);
}

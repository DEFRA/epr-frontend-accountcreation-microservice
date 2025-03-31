using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services;

public interface IReExAccountMapper
{
    ReExAccountModel CreateReExAccountModel(ReExAccountCreationSession session, string email);
}
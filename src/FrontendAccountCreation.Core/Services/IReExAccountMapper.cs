using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services;

public interface IReExAccountMapper
{
    ReprocessorExporterAccountModel CreateReprocessorExporterAccountModel(ReExAccountCreationSession session, string email);

    ReExAccountModel CreateReExAccountModel(ReExAccountCreationSession session, string email);
}
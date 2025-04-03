using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services;

public interface IAccountMapper
{
    AccountModel CreateAccountModel(AccountCreationSession session, string email);
    //todo: rename full reex?
    ReprocessorExporterAccountModel CreateReExAccountModel(AccountCreationSession session, string email);
    ReprocessorExporterAccountModel CreateReExAccountModel(ReExAccountCreationSession session, string email);
}
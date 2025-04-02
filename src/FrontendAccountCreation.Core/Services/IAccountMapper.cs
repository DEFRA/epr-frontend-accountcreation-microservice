using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services;

public interface IAccountMapper
{
    AccountModel CreateAccountModel(AccountCreationSession session, string email);
    PersonModel CreateReExAccountModel(AccountCreationSession session, string email);
}
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services;

public class ReExAccountMapper : IReExAccountMapper
{
    public ReprocessorExporterAccountModel CreateReprocessorExporterAccountModel(ReExAccountCreationSession session, string email)
    {
        return new ReprocessorExporterAccountModel
        {
            Person = new PersonModel
            {
                FirstName = session.Contact.FirstName,
                LastName = session.Contact.LastName,
                ContactEmail = email,
                TelephoneNumber = session.Contact.TelephoneNumber
            }
        };
    }

    public ReExAccountModel CreateReExAccountModel(ReExAccountCreationSession session, string email)
    {
        var person = new ReExPersonModel()
        {
            FirstName = session.Contact.FirstName,
            LastName = session.Contact.LastName,
            ContactEmail = email,
            TelephoneNumber = session.Contact.TelephoneNumber
        };

        var account = new ReExAccountModel()
        {
            Person = person,
        };

        return account;
    }
}
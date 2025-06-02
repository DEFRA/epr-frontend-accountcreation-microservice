using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.Constants;

[ExcludeFromCodeCoverage]
public static class PagePath
{
    // Journey paths
    public const string RegisteredAsCharity = "registered-as-charity";
    public const string RegisteredWithCompaniesHouse = "registered-with-companies-house";
    public const string TypeOfOrganisation = "type-of-organisation";
    public const string CompaniesHouseNumber = "companies-house-number";
    public const string TradingName = "trading-name";
    public const string BusinessAddressPostcode = "business-address-postcode";
    public const string SelectBusinessAddress = "select-business-address";
    public const string BusinessAddress = "business-address";
    public const string ConfirmCompanyDetails = "confirm-company-details";
    public const string UkNation = "uk-nation";
    public const string CannotVerifyOrganisation = "cannot-verify-organisation";
    public const string RoleInOrganisation = "role-in-organisation";
    public const string ManualInputRoleInOrganisation = "manual-input-role-in-organisation";
    public const string FullName = "full-name";
    public const string TelephoneNumber = "telephone-number";
    public const string CannotCreateAccount = "cannot-create-account";
    public const string CheckYourDetails = "check-your-details";
    public const string Declaration = "declaration";
    public const string DeclarationWithFullName = "declarationWithFullName";
    public const string ReportPackagingData = "report-packaging-data";
    public const string NotAffected = "not-affected";
    public const string AccountAlreadyExists = "account-already-exists";
    public const string UserAlreadyExists = "user-already-exists";
    public const string Invitation = "invitation";
    public const string InviteeFullName = "invitee-full-name";
    public const string Success = "success";
    public const string IsUkMainAddress = "is-uk-main-address";
    public const string IsTradingNameDifferent = "confirm-trading-name";
    public const string IsPartnership = "is-partnership";
    public const string PartnerOrganisation = "partner-organisation";
    public const string ManageAccountPerson = "manage-account-person";
    public const string YouAreApprovedPerson = "approved-person";
    public const string AddApprovedPerson = "add-approved-person";
    public const string ApprovedPersonContinue = "person-approved-continue";
    public const string DeclarationContinue = "declaration-continue";

    // Approve person paths
    public const string AddAnApprovedPerson = "add-an-approved-person";
    public const string TeamMemberRoleInOrganisation = "check-companies-house-role";
    public const string TeamMemberDetails = "eligible-person-details";
    public const string TeamMembersCheckInvitationDetails = "check-invitation-details";
    public const string ApprovedPersonPartnershipCanNotBeInvited = "approved-person-cannot-be-invited";

    // Limited partnership paths
    public const string LimitedPartnershipNamesOfPartners = "organisation-enter-corporate-individual-partner-names";
    public const string LimitedPartnershipCheckNamesOfPartners = "organisation-check-corporate-individual-partner-names";
    public const string PartnershipType= "partnership-type";
    public const string LimitedPartnershipType = "limited-partnership-type";
    public const string LimitedPartnershipRole = "organisation-your-role-in-limited-partnership";
    public const string LimitedPartnershipYouAreApprovedPerson = "you-are-now-an-approved-person";
    // Non journey paths 
    public const string Accessibility = "accessibility";    
    public const string SignedOut = "signed-out";
    public const string Error = "error";
    public const string AError = "auth-error";
    public const string ErrorReEx = "error-reex";
    public const string Culture = "culture";
    //to-do: this page path doesn't exist, but if you redirect to it, you'll get "page not found" as the path doesn't exist :-)
    public const string PageNotFound = "PageNotFound";
    public const string PageNotFoundReEx = "/page-not-found-reex";
    public const string AcknowledgeCookieAcceptance = "acknowledge-cookie-acceptance";
    public const string UpdateCookieAcceptance = "update-cookie-acceptance";
}

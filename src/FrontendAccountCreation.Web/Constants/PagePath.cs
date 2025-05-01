namespace FrontendAccountCreation.Web.Constants;

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
    public const string IsTradingNameDifferent = "confirm-trading-name";
    public const string IsPartnership = "is-partnership";
    public const string PartnerOrganisation = "partner-organisation";
    public const string ManageAccountPerson = "manage-account-person";

    //Approve person paths
    public const string AddAnApprovedPerson = "add-an-approved-person";
    public const string TeamMemberRoleInOrganisation = "check-companies-house-role";
    public const string TeamMemberDetails = "eligible-person-details";
    public const string TeamMembersCheckInvitationDetails = "check-invitation-details";

    // Non journey paths
    public const string Accessibility = "accessibility";
    public const string SignedOut = "signed-out";
    public const string Error = "error";
    public const string AError = "auth-error";
    public const string Culture = "culture";
    public const string PageNotFound = "PageNotFound";
    public const string AcknowledgeCookieAcceptance = "acknowledge-cookie-acceptance";
    public const string UpdateCookieAcceptance = "update-cookie-acceptance";
}

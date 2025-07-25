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

    //to-do: don't think this is used anymore
    public const string AddApprovedPerson = "add-approved-person";

    public const string ApprovedPersonContinue = "person-approved-continue";
    public const string DeclarationContinue = "declaration-continue";
    public const string SoleTrader = "sole-trader";
    public const string NotApprovedPerson = "not-approved-person";
    public const string NotImplemented = "not-implemented";
    public const string AddressOverseas = "address-overseas";
    public const string UkRegulator = "uk-regulator";
    public const string ManageControl = "manage-control";
    public const string ManageControlOrganisation = "manage-control-organisation";
    public const string IndividualIncharge = "individual-in-charge";

    // Approve person paths
    public const string AddAnApprovedPerson = "add-an-approved-person";

    public const string TeamMemberRoleInOrganisation = "check-companies-house-role";
    public const string TeamMemberRoleInOrganisationAddAnother = "check-companies-house-role/add-another";
    public const string TeamMemberRoleInOrganisationAdd = "check-companies-house-role/add";
    public const string TeamMemberRoleInOrganisationEdit = "check-companies-house-role/edit";
    public const string TeamMemberRoleInOrganisationContinueWithoutInvitation = "check-companies-house-role/continue-without-invitation";

    public const string TeamMemberDetails = "eligible-person-details";
    public const string TeamMemberDetailsEdit = "eligible-person-details/edit";

    public const string TeamMembersCheckInvitationDetails = "check-invitation-details";
    public const string TeamMembersCheckInvitationDetailsDelete = "check-invitation-details/delete";

    public const string ApprovedPersonCanNotBeInvited = "approved-person-cannot-be-invited";

    public const string MemberPartnership = "member-partnership";
    public const string MemberPartnershipAdd = "member-partnership/add";
    public const string MemberPartnershipEdit = "member-partnership/edit";

    public const string PartnerDetails = "partner-details";
    public const string CanNotInviteThisPerson = "cannot-invite-this-person";
    public const string CanNotInviteThisPersonAddEligible = "cannot-invite-this-person/add-eligble";
    public const string CheckPartnerInvitation = "check-partner-invitation";

    // Sole Trader paths
    public const string YouAreApprovedPersonSoleTrader = "soletrader-approved-person";

    public const string SoleTraderContinue = "soletrader-continue";

    // UK & Non-UK Non-Companies-House path
    public const string OrganisationName = "organisation-name";

    // Non Uk Non Companies House paths
    public const string NonUkRoleInOrganisation = "nonuk-role-in-organisation";

    // Non Companies House Partnership paths
    // page 1 Which type of partners are in your partnership?
    public const string NonCompaniesHousePartnershipType = "non-companies-house-partner-type";

    // page 2 What are the names of all partners in your partnership?
    public const string NonCompaniesHousePartnershipNamesOfPartners = "non-companies-house-partner-names";

    public const string NonCompaniesHousePartnershipNamesOfPartnersDelete = "non-companies-house-partner-names/delete";

    // page 3 Check corporate and individual partner names in your partnership
    public const string NonCompaniesHousePartnershipCheckNamesOfPartners = "non-companies-house-check-partner-names";

    public const string NonCompaniesHousePartnershipCheckNamesOfPartnersDelete = "non-companies-house-check-partner-names/delete";

    // page 4 What role do you have within the partnership?
    public const string NonCompaniesHousePartnershipYourRole = "non-companies-house-your-partnership-role";

    // page 5a Add an approved person
    public const string NonCompaniesHousePartnershipAddApprovedPerson = "non-companies-house-partnership-add-approved-person";

    // page 5b Add an approved person
    public const string NonCompaniesHousePartnershipInviteApprovedPerson = "non-companies-house-partnership-invite-approved-person";

    // page 6 What role do they have within the partnership?
    public const string NonCompaniesHousePartnershipTheirRole = "non-companies-house-partnership-role";

    public const string NonCompaniesHousePartnershipTheirRoleAdd = "non-companies-house-partnership-role/add";
    public const string NonCompaniesHousePartnershipTheirRoleAddAnother = "non-companies-house-partnership-role/add-another";
    public const string NonCompaniesHousePartnershipTheirRoleEdit = "non-companies-house-partnership-role/edit";

    // page 7 What are their details?
    public const string NonCompaniesHouseTeamMemberDetails = "non-companies-house-team-member-details";

    public const string NonCompaniesHouseTeamMemberDetailsEdit = "non-companies-house-team-member-details/edit";

    // page 8 Check invitation details
    public const string NonCompaniesHouseTeamMemberCheckInvitationDetails = "non-companies-house-check-invitation-details";

    public const string NonCompaniesHouseTeamMemberCheckInvitationDetailsDelete = "non-companies-house-check-invitation-details/delete";

    public const string NonCompaniesHousePartnershipYouAreApprovedPerson = "non-companies-house-partnership-approved-person-confirmation";

    // Limited partnership paths
    public const string LimitedPartnershipNamesOfPartners = "organisation-enter-corporate-individual-partner-names";
    public const string LimitedPartnershipNamesOfPartnersDelete = "organisation-enter-corporate-individual-partner-names/delete";

    public const string LimitedPartnershipCheckNamesOfPartners = "organisation-check-corporate-individual-partner-names";
    public const string LimitedPartnershipCheckNamesOfPartnersDelete = "organisation-check-corporate-individual-partner-names/delete";

    public const string PartnershipType = "partnership-type";
    public const string LimitedPartnershipType = "organisation-partnership-type";
    public const string LimitedPartnershipRole = "organisation-your-role-in-limited-partnership";
    public const string LimitedLiabilityPartnership = "partner-you";

    // Unincorporated paths
    public const string UnincorporatedRoleInOrganisation = "unincorporated-role-in-organisation";

    // Non journey paths
    public const string Accessibility = "accessibility";

    public const string SignedOut = "signed-out";
    public const string Error = "error";
    public const string AError = "auth-error";
    public const string ErrorReEx = "error-reex";
    public const string Culture = "culture";
    public const string TimeoutSignedOut = "timeout-signed-out";

    public const string PageNotFound = "page-not-found";

    public const string PageNotFoundReEx = "/page-not-found-reex";
    public const string AcknowledgeCookieAcceptance = "acknowledge-cookie-acceptance";
    public const string UpdateCookieAcceptance = "update-cookie-acceptance";
}
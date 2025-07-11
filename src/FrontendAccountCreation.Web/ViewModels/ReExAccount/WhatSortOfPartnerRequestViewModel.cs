﻿using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class WhatSortOfPartnerRequestViewModel : IValidatableObject
{
    public bool HasIndividualPartners { get; set; }
    public bool HasCompanyPartners { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasIndividualPartners && !HasCompanyPartners)
        {
            yield return new ValidationResult(
                "Partial._WhatSortOfPartnerCheckBoxes.ErrorMessage",
                new[] { nameof(HasIndividualPartners) }  // Only attach to one field to avoid duplicate messages
            );
        }
    }
}
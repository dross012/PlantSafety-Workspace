// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for VerifyComprehensionSiteOrientationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.VerifyComprehensionSiteOrientationCommand"/>.</summary>
public sealed class VerifyComprehensionSiteOrientationCommandValidator : AbstractValidator<Commands.VerifyComprehensionSiteOrientationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.VerifyComprehensionSiteOrientationCommand"/>.</summary>
    public VerifyComprehensionSiteOrientationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteOrientationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ComprehensionScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.ValidityDays).GreaterThanOrEqualTo(0);
    }
}
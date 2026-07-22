// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishPpePoliciesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.PublishPpePoliciesCommand"/>.</summary>
public sealed class PublishPpePoliciesCommandValidator : AbstractValidator<Commands.PublishPpePoliciesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishPpePoliciesCommand"/>.</summary>
    public PublishPpePoliciesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PpePoliciesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.ReplacementScheduleCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReviewCycleDays).GreaterThanOrEqualTo(0);
    }
}
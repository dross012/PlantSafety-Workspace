// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleRevisionPpePoliciesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.ScheduleRevisionPpePoliciesCommand"/>.</summary>
public sealed class ScheduleRevisionPpePoliciesCommandValidator : AbstractValidator<Commands.ScheduleRevisionPpePoliciesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleRevisionPpePoliciesCommand"/>.</summary>
    public ScheduleRevisionPpePoliciesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PpePoliciesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
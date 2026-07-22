// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordEnforcementAuditPpePoliciesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RecordEnforcementAuditPpePoliciesCommand"/>.</summary>
public sealed class RecordEnforcementAuditPpePoliciesCommandValidator : AbstractValidator<Commands.RecordEnforcementAuditPpePoliciesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordEnforcementAuditPpePoliciesCommand"/>.</summary>
    public RecordEnforcementAuditPpePoliciesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PpePoliciesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m);
    }
}
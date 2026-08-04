// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for IssueDischargePermitCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.IssueDischargePermitCommand"/>.</summary>
public sealed class IssueDischargePermitCommandValidator : AbstractValidator<Commands.IssueDischargePermitCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.IssueDischargePermitCommand"/>.</summary>
    public IssueDischargePermitCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IssuingAuthority).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExpiresAtUtc).GreaterThan(x => x.EffectiveAtUtc);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
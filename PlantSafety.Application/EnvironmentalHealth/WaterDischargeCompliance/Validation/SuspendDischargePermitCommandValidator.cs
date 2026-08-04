// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for SuspendDischargePermitCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.SuspendDischargePermitCommand"/>.</summary>
public sealed class SuspendDischargePermitCommandValidator : AbstractValidator<Commands.SuspendDischargePermitCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SuspendDischargePermitCommand"/>.</summary>
    public SuspendDischargePermitCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePermitId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
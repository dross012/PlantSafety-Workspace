// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for VoidEffluentSampleCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.VoidEffluentSampleCommand"/>.</summary>
public sealed class VoidEffluentSampleCommandValidator : AbstractValidator<Commands.VoidEffluentSampleCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.VoidEffluentSampleCommand"/>.</summary>
    public VoidEffluentSampleCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EffluentSampleId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
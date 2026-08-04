// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordLimitExceedanceCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.RecordLimitExceedanceCommand"/>.</summary>
public sealed class RecordLimitExceedanceCommandValidator : AbstractValidator<Commands.RecordLimitExceedanceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordLimitExceedanceCommand"/>.</summary>
    public RecordLimitExceedanceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePointId).NotEmpty();
        RuleFor(x => x.EffluentSampleId).NotEmpty();
        RuleFor(x => x.DischargeLimitId).NotEmpty();
        RuleFor(x => x.ParameterName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
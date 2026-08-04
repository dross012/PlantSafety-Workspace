// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordEffluentSampleCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.RecordEffluentSampleCommand"/>.</summary>
public sealed class RecordEffluentSampleCommandValidator : AbstractValidator<Commands.RecordEffluentSampleCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordEffluentSampleCommand"/>.</summary>
    public RecordEffluentSampleCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePointId).NotEmpty();
        RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CollectedByUserId).NotEmpty();
        RuleFor(x => x.LabSourceSystem).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ChainOfCustodyReference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
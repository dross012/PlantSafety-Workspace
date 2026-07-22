// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordCalibrationResultPresenceSensingDevicesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordCalibrationResultPresenceSensingDevicesCommand"/>.</summary>
public sealed class RecordCalibrationResultPresenceSensingDevicesCommandValidator : AbstractValidator<Commands.RecordCalibrationResultPresenceSensingDevicesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordCalibrationResultPresenceSensingDevicesCommand"/>.</summary>
    public RecordCalibrationResultPresenceSensingDevicesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PresenceSensingDevicesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.DetectionAccuracyScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.CalibratedByUserId).NotEmpty();
    }
}
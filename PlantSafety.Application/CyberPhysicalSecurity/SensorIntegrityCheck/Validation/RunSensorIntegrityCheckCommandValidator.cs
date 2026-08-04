// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RunSensorIntegrityCheckCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Validation;

/// <summary>Validates <see cref="Commands.RunSensorIntegrityCheckCommand"/>.</summary>
public sealed class RunSensorIntegrityCheckCommandValidator : AbstractValidator<Commands.RunSensorIntegrityCheckCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RunSensorIntegrityCheckCommand"/>.</summary>
    public RunSensorIntegrityCheckCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.OtAssetId).NotEmpty();
        RuleFor(x => x.CheckType).NotEmpty();
        RuleFor(x => x.Result).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

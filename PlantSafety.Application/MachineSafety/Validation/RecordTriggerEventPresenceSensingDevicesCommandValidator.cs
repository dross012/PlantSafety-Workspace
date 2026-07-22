// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordTriggerEventPresenceSensingDevicesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordTriggerEventPresenceSensingDevicesCommand"/>.</summary>
public sealed class RecordTriggerEventPresenceSensingDevicesCommandValidator : AbstractValidator<Commands.RecordTriggerEventPresenceSensingDevicesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordTriggerEventPresenceSensingDevicesCommand"/>.</summary>
    public RecordTriggerEventPresenceSensingDevicesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PresenceSensingDevicesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordControlChangeEventCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Validation;

/// <summary>Validates <see cref="Commands.RecordControlChangeEventCommand"/>.</summary>
public sealed class RecordControlChangeEventCommandValidator : AbstractValidator<Commands.RecordControlChangeEventCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordControlChangeEventCommand"/>.</summary>
    public RecordControlChangeEventCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.OtAssetId).NotEmpty();
        RuleFor(x => x.ChangedParameter).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

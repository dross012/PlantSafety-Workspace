// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordLiftEventCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordLiftEventCommand"/>.</summary>
public sealed class RecordLiftEventCommandValidator : AbstractValidator<Commands.RecordLiftEventCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordLiftEventCommand"/>.</summary>
    public RecordLiftEventCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CraneHoistSystemsId).NotEmpty();
        RuleFor(x => x.ObservedLoadKg).GreaterThan(0m);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

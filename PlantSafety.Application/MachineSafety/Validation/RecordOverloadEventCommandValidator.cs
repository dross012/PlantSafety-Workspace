// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordOverloadEventCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordOverloadEventCommand"/>.</summary>
public sealed class RecordOverloadEventCommandValidator : AbstractValidator<Commands.RecordOverloadEventCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordOverloadEventCommand"/>.</summary>
    public RecordOverloadEventCommandValidator()
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

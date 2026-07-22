// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: FluentValidation validator for CreateStopWorkEventCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyCulture.Validation;

/// <summary>Validates <see cref="Commands.CreateStopWorkEventCommand"/>.</summary>
public sealed class CreateStopWorkEventCommandValidator : AbstractValidator<Commands.CreateStopWorkEventCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateStopWorkEventCommand"/>.</summary>
    public CreateStopWorkEventCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.WorkArea).NotEmpty().MaximumLength(256);
        RuleFor(x => x.HazardDescription).NotEmpty().MaximumLength(2000);
    }
}
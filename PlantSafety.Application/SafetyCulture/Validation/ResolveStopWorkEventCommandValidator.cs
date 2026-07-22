// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ResolveStopWorkEventCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyCulture.Validation;

/// <summary>Validates <see cref="Commands.ResolveStopWorkEventCommand"/>.</summary>
public sealed class ResolveStopWorkEventCommandValidator : AbstractValidator<Commands.ResolveStopWorkEventCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ResolveStopWorkEventCommand"/>.</summary>
    public ResolveStopWorkEventCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.StopWorkEventId).NotEmpty();
        RuleFor(x => x.ResolutionSummary).NotEmpty().MaximumLength(2048);
    }
}

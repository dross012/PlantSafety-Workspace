// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- validates the real reporting-period fields (PeriodType, PeriodStartUtc).
// FILE DESCRIPTION: FluentValidation validator for CreateScorecardCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 2

using FluentValidation;
using PlantSafety.Domain.Scorecards.Enums;

namespace PlantSafety.Application.Scorecards.Validation;

/// <summary>Validates <see cref="Commands.CreateScorecardCommand"/>.</summary>
public sealed class CreateScorecardCommandValidator : AbstractValidator<Commands.CreateScorecardCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateScorecardCommand"/>.</summary>
    public CreateScorecardCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.ScorecardTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PeriodStartUtc).NotEmpty();
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(IndicatorCategory.IsValid)
            .WithMessage("Category must be a recognized IndicatorCategory code.");
        RuleFor(x => x.PeriodType)
            .NotEmpty()
            .Must(ScorecardPeriodType.IsValid)
            .WithMessage("PeriodType must be a recognized ScorecardPeriodType code.");
    }
}

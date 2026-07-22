// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- extended to validate the real report-time fields (location, severity,
//   injured person count, occurrence time).
// FILE DESCRIPTION: FluentValidation validator for CreateAccidentCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 2

using FluentValidation;
using PlantSafety.Domain.AccidentManagement.Enums;

namespace PlantSafety.Application.AccidentManagement.Validation;

/// <summary>Validates <see cref="Commands.CreateAccidentCommand"/>.</summary>
public sealed class CreateAccidentCommandValidator : AbstractValidator<Commands.CreateAccidentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateAccidentCommand"/>.</summary>
    public CreateAccidentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.AccidentTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(256);
        RuleFor(x => x.InjuredPersonCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OccurredAtUtc).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5));
        RuleFor(x => x.Severity)
            .NotEmpty()
            .Must(AccidentSeverity.IsValid)
            .WithMessage("Severity must be a recognized AccidentSeverity code.");
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(InvestigationPhase.IsValid)
            .WithMessage("Category must be a recognized InvestigationPhase code.");
    }
}

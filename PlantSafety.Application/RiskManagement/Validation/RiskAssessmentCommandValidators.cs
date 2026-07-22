// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validators for the real 3-phase Risk Management workflow
//   commands. Enforce invariants and normalization boundaries only; never call repositories or
//   external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.RiskManagement.Enums;

namespace PlantSafety.Application.RiskManagement.Validation;

/// <summary>Validates <see cref="Commands.ScoreRiskCommand"/>.</summary>
public sealed class ScoreRiskCommandValidator : AbstractValidator<Commands.ScoreRiskCommand>
{
    /// <summary>Defines validation rules.</summary>
    public ScoreRiskCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskId).NotEmpty();
        RuleFor(x => x.ScoredBy).NotEmpty();
        RuleFor(x => x.Severity).InclusiveBetween(1, 5);
        RuleFor(x => x.Likelihood).InclusiveBetween(1, 5);
    }
}

/// <summary>Validates <see cref="Commands.PrioritizeRiskCommand"/>.</summary>
public sealed class PrioritizeRiskCommandValidator : AbstractValidator<Commands.PrioritizeRiskCommand>
{
    /// <summary>Defines validation rules.</summary>
    public PrioritizeRiskCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskId).NotEmpty();
        RuleFor(x => x.PrioritizedBy).NotEmpty();
        RuleFor(x => x.Priority)
            .Must(code => string.IsNullOrWhiteSpace(code) || RiskPriority.IsValid(code))
            .WithMessage("Priority must be a recognized P1-P4 RiskPriority code when supplied.");
    }
}

/// <summary>Validates <see cref="Commands.ApplyRiskControlMeasureCommand"/>.</summary>
public sealed class ApplyRiskControlMeasureCommandValidator : AbstractValidator<Commands.ApplyRiskControlMeasureCommand>
{
    /// <summary>Defines validation rules.</summary>
    public ApplyRiskControlMeasureCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskId).NotEmpty();
        RuleFor(x => x.ResponsiblePersonId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.HierarchyLevel)
            .NotEmpty()
            .Must(ControlHierarchyLevel.IsValid)
            .WithMessage("HierarchyLevel must be a recognized ControlHierarchyLevel code.");
        RuleFor(x => x.Justification)
            .NotEmpty()
            .When(x => ControlHierarchyLevel.IsValid(x.HierarchyLevel) && ControlHierarchyLevel.From(x.HierarchyLevel).RequiresJustification)
            .WithMessage("Justification is required when applying a control below Elimination/Substitution on the Hierarchy of Controls.");
    }
}

/// <summary>Validates <see cref="Commands.ValidateRiskEffectivenessCommand"/>.</summary>
public sealed class ValidateRiskEffectivenessCommandValidator : AbstractValidator<Commands.ValidateRiskEffectivenessCommand>
{
    /// <summary>Defines validation rules.</summary>
    public ValidateRiskEffectivenessCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskId).NotEmpty();
        RuleFor(x => x.ValidatedBy).NotEmpty();
        RuleFor(x => x.ResidualSeverity).InclusiveBetween(1, 5);
        RuleFor(x => x.ResidualLikelihood).InclusiveBetween(1, 5);
    }
}

/// <summary>Validates <see cref="Commands.StartRiskReviewCycleCommand"/>.</summary>
public sealed class StartRiskReviewCycleCommandValidator : AbstractValidator<Commands.StartRiskReviewCycleCommand>
{
    /// <summary>Defines validation rules.</summary>
    public StartRiskReviewCycleCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskId).NotEmpty();
        RuleFor(x => x.StartedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.CompleteRiskReviewCommand"/>.</summary>
public sealed class CompleteRiskReviewCommandValidator : AbstractValidator<Commands.CompleteRiskReviewCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CompleteRiskReviewCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskId).NotEmpty();
        RuleFor(x => x.ReviewedBy).NotEmpty();
        RuleFor(x => x.NewReviewCadence)
            .Must(code => string.IsNullOrWhiteSpace(code) || RiskReviewCadence.IsValid(code))
            .WithMessage("NewReviewCadence must be a recognized RiskReviewCadence code when supplied.");
    }
}

/// <summary>Validates <see cref="Commands.CloseRiskCommand"/>.</summary>
public sealed class CloseRiskCommandValidator : AbstractValidator<Commands.CloseRiskCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CloseRiskCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskId).NotEmpty();
        RuleFor(x => x.ClosedBy).NotEmpty();
    }
}

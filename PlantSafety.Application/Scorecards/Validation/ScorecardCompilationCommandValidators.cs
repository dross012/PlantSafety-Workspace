// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validators for the real Scorecard compilation/distribution
//   workflow commands. Enforce invariants and normalization boundaries only; never call
//   repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.Scorecards.Enums;

namespace PlantSafety.Application.Scorecards.Validation;

/// <summary>Validates <see cref="Commands.RecordScorecardMetricCommand"/>.</summary>
public sealed class RecordScorecardMetricCommandValidator : AbstractValidator<Commands.RecordScorecardMetricCommand>
{
    /// <summary>Defines validation rules.</summary>
    public RecordScorecardMetricCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ScorecardId).NotEmpty();
        RuleFor(x => x.RecordedBy).NotEmpty();
        RuleFor(x => x.DataSourceModule).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Numerator).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Denominator).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Metric)
            .NotEmpty()
            .Must(KpiMetricDefinition.IsValid)
            .WithMessage("Metric must be a recognized KpiMetricDefinition code.");
    }
}

/// <summary>Validates <see cref="Commands.CompleteScorecardCompilationCommand"/>.</summary>
public sealed class CompleteScorecardCompilationCommandValidator : AbstractValidator<Commands.CompleteScorecardCompilationCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CompleteScorecardCompilationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ScorecardId).NotEmpty();
        RuleFor(x => x.CompletedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.PublishScorecardCommand"/>.</summary>
public sealed class PublishScorecardCommandValidator : AbstractValidator<Commands.PublishScorecardCommand>
{
    /// <summary>Defines validation rules.</summary>
    public PublishScorecardCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ScorecardId).NotEmpty();
        RuleFor(x => x.PublishedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="Commands.RecordScorecardDistributionCommand"/>.</summary>
public sealed class RecordScorecardDistributionCommandValidator : AbstractValidator<Commands.RecordScorecardDistributionCommand>
{
    /// <summary>Defines validation rules.</summary>
    public RecordScorecardDistributionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ScorecardId).NotEmpty();
        RuleFor(x => x.DistributedBy).NotEmpty();
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(128);
    }
}

/// <summary>Validates <see cref="Commands.CloseScorecardCommand"/>.</summary>
public sealed class CloseScorecardCommandValidator : AbstractValidator<Commands.CloseScorecardCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CloseScorecardCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ScorecardId).NotEmpty();
        RuleFor(x => x.ClosedBy).NotEmpty();
    }
}

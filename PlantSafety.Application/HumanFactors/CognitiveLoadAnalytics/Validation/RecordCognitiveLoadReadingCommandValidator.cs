// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           RecordCognitiveLoadReadingCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecordCognitiveLoadReadingCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

/// <summary>Validates <see cref="Commands.RecordCognitiveLoadReadingCommand"/>.</summary>
public sealed class RecordCognitiveLoadReadingCommandValidator : AbstractValidator<Commands.RecordCognitiveLoadReadingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordCognitiveLoadReadingCommand"/>.</summary>
    public RecordCognitiveLoadReadingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.WorkerUserId).NotEmpty();
        RuleFor(x => x.ShiftType).NotEmpty().Must(s => Enum.TryParse<Domain.HumanFactors.CognitiveLoadAnalytics.Enums.ShiftType>(s, true, out _))
            .WithMessage("ShiftType must be one of: Day, Evening, Night, Rotating.");
        RuleFor(x => x.CognitiveLoadScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.MeasurementMethod).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MeasuredAtUtc).LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5));
    }
}

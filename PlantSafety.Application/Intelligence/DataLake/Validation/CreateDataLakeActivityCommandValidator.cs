// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           CreateDataLakeActivityCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateDataLakeActivityCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Validation
//                  .CreateRCACommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Intelligence.DataLake.Validation;

/// <summary>Validates <see cref="Commands.CreateDataLakeActivityCommand"/>.</summary>
public sealed class CreateDataLakeActivityCommandValidator : AbstractValidator<Commands.CreateDataLakeActivityCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateDataLakeActivityCommand"/>.</summary>
    public CreateDataLakeActivityCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DatasetName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Zone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RecordCount).GreaterThanOrEqualTo(0).When(x => x.RecordCount.HasValue);
        RuleFor(x => x.ProcessedByUserId).NotEmpty();
    }
}

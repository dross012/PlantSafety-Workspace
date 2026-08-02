// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           CreateEnvironmentalHealthCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateEnvironmentalHealthCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.Validation;

/// <summary>Validates <see cref="Commands.CreateEnvironmentalHealthCommand"/>.</summary>
public sealed class CreateEnvironmentalHealthCommandValidator : AbstractValidator<Commands.CreateEnvironmentalHealthCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateEnvironmentalHealthCommand"/>.</summary>
    public CreateEnvironmentalHealthCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.MonitoringType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EvaluatedStatus).NotEmpty().MaximumLength(50);
    }
}

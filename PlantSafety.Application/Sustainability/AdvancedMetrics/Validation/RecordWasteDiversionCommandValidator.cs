// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordWasteDiversionCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecordWasteDiversionCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Validation;

/// <summary>Validates <see cref="Commands.RecordWasteDiversionCommand"/>.</summary>
public sealed class RecordWasteDiversionCommandValidator : AbstractValidator<Commands.RecordWasteDiversionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordWasteDiversionCommand"/>.</summary>
    public RecordWasteDiversionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ReportingPeriodEndUtc).GreaterThan(x => x.ReportingPeriodStartUtc);
        RuleFor(x => x.TotalWasteKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DivertedWasteKg).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.TotalWasteKg);
        RuleFor(x => x.MethodologyVersion).NotEmpty().MaximumLength(100);
    }
}

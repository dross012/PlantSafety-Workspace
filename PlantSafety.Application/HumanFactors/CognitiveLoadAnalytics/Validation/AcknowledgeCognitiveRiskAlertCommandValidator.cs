// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           AcknowledgeCognitiveRiskAlertCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for AcknowledgeCognitiveRiskAlertCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

/// <summary>Validates <see cref="Commands.AcknowledgeCognitiveRiskAlertCommand"/>.</summary>
public sealed class AcknowledgeCognitiveRiskAlertCommandValidator : AbstractValidator<Commands.AcknowledgeCognitiveRiskAlertCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AcknowledgeCognitiveRiskAlertCommand"/>.</summary>
    public AcknowledgeCognitiveRiskAlertCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CognitiveRiskPredictionId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.AcknowledgementNotes).MaximumLength(1000);
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           LogLiaisonCommunicationCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for LogLiaisonCommunicationCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

/// <summary>Validates <see cref="Commands.LogLiaisonCommunicationCommand"/>.</summary>
public sealed class LogLiaisonCommunicationCommandValidator : AbstractValidator<Commands.LogLiaisonCommunicationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.LogLiaisonCommunicationCommand"/>.</summary>
    public LogLiaisonCommunicationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LocalEmergencyCoordinationId).NotEmpty();
        RuleFor(x => x.ExternalResponderLiaisonId).NotEmpty();
        RuleFor(x => x.LoggedByUserId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

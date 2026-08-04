// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           DeactivateJointResponsePlanCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for DeactivateJointResponsePlanCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

/// <summary>Validates <see cref="Commands.DeactivateJointResponsePlanCommand"/>.</summary>
public sealed class DeactivateJointResponsePlanCommandValidator : AbstractValidator<Commands.DeactivateJointResponsePlanCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeactivateJointResponsePlanCommand"/>.</summary>
    public DeactivateJointResponsePlanCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LocalEmergencyCoordinationId).NotEmpty();
        RuleFor(x => x.JointResponsePlanId).NotEmpty();
        RuleFor(x => x.DeactivatedByUserId).NotEmpty();
    }
}

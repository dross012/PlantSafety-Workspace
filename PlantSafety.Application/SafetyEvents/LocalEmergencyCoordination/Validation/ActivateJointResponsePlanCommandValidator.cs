// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ActivateJointResponsePlanCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ActivateJointResponsePlanCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

/// <summary>Validates <see cref="Commands.ActivateJointResponsePlanCommand"/>.</summary>
public sealed class ActivateJointResponsePlanCommandValidator : AbstractValidator<Commands.ActivateJointResponsePlanCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ActivateJointResponsePlanCommand"/>.</summary>
    public ActivateJointResponsePlanCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LocalEmergencyCoordinationId).NotEmpty();
        RuleFor(x => x.ActivatedByUserId).NotEmpty();
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(200);
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           AssignExternalResponderLiaisonCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for AssignExternalResponderLiaisonCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

/// <summary>Validates <see cref="Commands.AssignExternalResponderLiaisonCommand"/>.</summary>
public sealed class AssignExternalResponderLiaisonCommandValidator : AbstractValidator<Commands.AssignExternalResponderLiaisonCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AssignExternalResponderLiaisonCommand"/>.</summary>
    public AssignExternalResponderLiaisonCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LocalEmergencyCoordinationId).NotEmpty();
        RuleFor(x => x.LiaisonUserId).NotEmpty();
        RuleFor(x => x.AssignedByUserId).NotEmpty();
        RuleFor(x => x.AgencyName).NotEmpty().MaximumLength(200);
    }
}

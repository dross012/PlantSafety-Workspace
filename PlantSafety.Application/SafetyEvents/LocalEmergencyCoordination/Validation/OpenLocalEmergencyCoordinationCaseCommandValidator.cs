// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           OpenLocalEmergencyCoordinationCaseCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for OpenLocalEmergencyCoordinationCaseCommand.
//                  Enforces invariants and normalization boundaries only; never calls
//                  repositories or external services (Handbook Rule 2.3). Mirrors
//                  DeclareEmergencyCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

/// <summary>Validates <see cref="Commands.OpenLocalEmergencyCoordinationCaseCommand"/>.</summary>
public sealed class OpenLocalEmergencyCoordinationCaseCommandValidator : AbstractValidator<Commands.OpenLocalEmergencyCoordinationCaseCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.OpenLocalEmergencyCoordinationCaseCommand"/>.</summary>
    public OpenLocalEmergencyCoordinationCaseCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OpenedByUserId).NotEmpty();
        RuleFor(x => x.CaseName).NotEmpty().MaximumLength(200);
    }
}

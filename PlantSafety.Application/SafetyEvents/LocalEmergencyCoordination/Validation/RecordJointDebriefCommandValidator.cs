// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           RecordJointDebriefCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecordJointDebriefCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

/// <summary>Validates <see cref="Commands.RecordJointDebriefCommand"/>.</summary>
public sealed class RecordJointDebriefCommandValidator : AbstractValidator<Commands.RecordJointDebriefCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordJointDebriefCommand"/>.</summary>
    public RecordJointDebriefCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LocalEmergencyCoordinationId).NotEmpty();
        RuleFor(x => x.RecordedByUserId).NotEmpty();
        RuleFor(x => x.AgenciesInvolved).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
    }
}

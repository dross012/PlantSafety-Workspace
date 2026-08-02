// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           RaisePredictiveMaintenanceAlertCommandValidator.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RaisePredictiveMaintenanceAlertCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Validation;

/// <summary>Validates <see cref="Commands.RaisePredictiveMaintenanceAlertCommand"/>.</summary>
public sealed class RaisePredictiveMaintenanceAlertCommandValidator : AbstractValidator<Commands.RaisePredictiveMaintenanceAlertCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RaisePredictiveMaintenanceAlertCommand"/>.</summary>
    public RaisePredictiveMaintenanceAlertCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.PredictedFailureType).NotEmpty().MaximumLength(400);
        RuleFor(x => x.PredictedAtUtc).NotEmpty();
        RuleFor(x => x.RaisedByUserId).NotEmpty();
    }
}

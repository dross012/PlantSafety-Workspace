// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           CreateWorkerWellBeingCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateWorkerWellBeingCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.WorkerWellBeing.Validation;

/// <summary>Validates <see cref="Commands.CreateWorkerWellBeingCommand"/>.</summary>
public sealed class CreateWorkerWellBeingCommandValidator : AbstractValidator<Commands.CreateWorkerWellBeingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateWorkerWellBeingCommand"/>.</summary>
    public CreateWorkerWellBeingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.WorkerUserId).NotEmpty();
        RuleFor(x => x.WellBeingRiskLevel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CheckInType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WellBeingScore).InclusiveBetween(1, 10);
    }
}

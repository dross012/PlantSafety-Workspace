// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           CreateProcessDefinitionCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateProcessDefinitionCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.ProcessManagement.Validation;

/// <summary>Validates <see cref="Commands.CreateProcessDefinitionCommand"/>.</summary>
public sealed class CreateProcessDefinitionCommandValidator : AbstractValidator<Commands.CreateProcessDefinitionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateProcessDefinitionCommand"/>.</summary>
    public CreateProcessDefinitionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.ProcessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

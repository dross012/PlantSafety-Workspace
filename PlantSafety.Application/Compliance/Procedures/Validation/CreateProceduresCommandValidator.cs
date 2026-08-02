// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           CreateProceduresCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateProceduresCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.Procedures.Validation;

/// <summary>Validates <see cref="Commands.CreateProceduresCommand"/>.</summary>
public sealed class CreateProceduresCommandValidator : AbstractValidator<Commands.CreateProceduresCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateProceduresCommand"/>.</summary>
    public CreateProceduresCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ProcedureTitle).NotEmpty().MaximumLength(400);
        RuleFor(x => x.ProcedureNumber).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

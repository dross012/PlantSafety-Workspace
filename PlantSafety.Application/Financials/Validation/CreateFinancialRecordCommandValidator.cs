// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           CreateFinancialRecordCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateFinancialRecordCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation
//                  .CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Financials.Validation;

/// <summary>Validates <see cref="Commands.CreateFinancialRecordCommand"/>.</summary>
public sealed class CreateFinancialRecordCommandValidator : AbstractValidator<Commands.CreateFinancialRecordCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateFinancialRecordCommand"/>.</summary>
    public CreateFinancialRecordCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}

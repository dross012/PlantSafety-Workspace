// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           CreateContractorVisitorCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateContractorVisitorCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.ContractorVisitor.Validation;

/// <summary>Validates <see cref="Commands.CreateContractorVisitorCommand"/>.</summary>
public sealed class CreateContractorVisitorCommandValidator : AbstractValidator<Commands.CreateContractorVisitorCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateContractorVisitorCommand"/>.</summary>
    public CreateContractorVisitorCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.HostUserId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.VisitorType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PurposeOfVisit).NotEmpty().MaximumLength(2000);
    }
}

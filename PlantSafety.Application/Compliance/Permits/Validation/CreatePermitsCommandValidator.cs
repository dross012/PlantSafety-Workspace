// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           CreatePermitsCommandValidator.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreatePermitsCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.RiskManagement.Validation.CreateRiskCommandValidator.
//                  Additive-only evolution.
//
//                  NOTE (2026-08-05): the cross-domain "permit requires training" check
//                  deliberately does NOT live here. A newly-created Permits record starts in
//                  Status=Created (see Permits.Create in the Domain aggregate), not Active --
//                  training is only meaningful to check once the permit is actually about to
//                  authorize work, i.e. on ApprovePermitsCommand (Created -> Active). See
//                  ApprovePermitsCommandValidator.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.Permits.Validation;

/// <summary>Validates <see cref="Commands.CreatePermitsCommand"/>.</summary>
public sealed class CreatePermitsCommandValidator : AbstractValidator<Commands.CreatePermitsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreatePermitsCommand"/>.</summary>
    public CreatePermitsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.PermitType).NotEmpty().MaximumLength(400);
        RuleFor(x => x.PermitNumber).NotEmpty().MaximumLength(400);
        RuleFor(x => x.WorkDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(400);
        RuleFor(x => x.HazardsIdentified).NotEmpty().MaximumLength(2000);
    }
}

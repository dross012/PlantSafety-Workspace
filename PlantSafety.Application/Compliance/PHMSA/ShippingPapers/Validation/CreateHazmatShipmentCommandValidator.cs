// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           CreateHazmatShipmentCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateHazmatShipmentCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Validation;

/// <summary>Validates <see cref="Commands.CreateHazmatShipmentCommand"/>.</summary>
public sealed class CreateHazmatShipmentCommandValidator : AbstractValidator<Commands.CreateHazmatShipmentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateHazmatShipmentCommand"/>.</summary>
    public CreateHazmatShipmentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.MaterialIds).NotNull().Must(m => m.Count > 0)
            .WithMessage("At least one material is required to create a shipment.");
        RuleFor(x => x.Mode).NotEmpty().MaximumLength(10);
    }
}

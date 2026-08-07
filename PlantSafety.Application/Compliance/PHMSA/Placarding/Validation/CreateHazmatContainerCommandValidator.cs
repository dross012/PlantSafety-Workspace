// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           CreateHazmatContainerCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateHazmatContainerCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Validation;

/// <summary>Validates <see cref="Commands.CreateHazmatContainerCommand"/>.</summary>
public sealed class CreateHazmatContainerCommandValidator : AbstractValidator<Commands.CreateHazmatContainerCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateHazmatContainerCommand"/>.</summary>
    public CreateHazmatContainerCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.ContainerType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.PackagingSpecId).NotEmpty();
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           CreateDigitalTwinCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateDigitalTwinCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.DigitalTwin.Validation;

/// <summary>Validates <see cref="Commands.CreateDigitalTwinCommand"/>.</summary>
public sealed class CreateDigitalTwinCommandValidator : AbstractValidator<Commands.CreateDigitalTwinCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateDigitalTwinCommand"/>.</summary>
    public CreateDigitalTwinCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.TwinName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ModelType).NotEmpty().MaximumLength(100);
    }
}

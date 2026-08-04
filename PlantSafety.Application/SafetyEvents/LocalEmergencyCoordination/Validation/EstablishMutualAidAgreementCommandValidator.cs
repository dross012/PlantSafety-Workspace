// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           EstablishMutualAidAgreementCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for EstablishMutualAidAgreementCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

/// <summary>Validates <see cref="Commands.EstablishMutualAidAgreementCommand"/>.</summary>
public sealed class EstablishMutualAidAgreementCommandValidator : AbstractValidator<Commands.EstablishMutualAidAgreementCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.EstablishMutualAidAgreementCommand"/>.</summary>
    public EstablishMutualAidAgreementCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LocalEmergencyCoordinationId).NotEmpty();
        RuleFor(x => x.EstablishedByUserId).NotEmpty();
        RuleFor(x => x.AgencyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Discipline).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DocumentVersion).GreaterThanOrEqualTo(1);
        RuleFor(x => x.RenewalDueAtUtc).GreaterThan(x => x.EffectiveFromUtc);
    }
}

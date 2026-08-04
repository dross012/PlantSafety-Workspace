// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for InitiateExceedanceResponseCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.InitiateExceedanceResponseCommand"/>.</summary>
public sealed class InitiateExceedanceResponseCommandValidator : AbstractValidator<Commands.InitiateExceedanceResponseCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.InitiateExceedanceResponseCommand"/>.</summary>
    public InitiateExceedanceResponseCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LimitExceedanceId).NotEmpty();
        RuleFor(x => x.ResponseUserId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for ValidateSafetyCaseCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.AutonomousVehicleSafety.Validation;

/// <summary>Validates <see cref="Commands.ValidateSafetyCaseCommand"/>.</summary>
public sealed class ValidateSafetyCaseCommandValidator : AbstractValidator<Commands.ValidateSafetyCaseCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ValidateSafetyCaseCommand"/>.</summary>
    public ValidateSafetyCaseCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutonomousVehicleAssetId).NotEmpty();
        RuleFor(x => x.StandardReference).NotEmpty();
        RuleFor(x => x.DocumentVersion).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RevalidationCadenceDays).GreaterThan(0);
        RuleFor(x => x.ValidatedByUserId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

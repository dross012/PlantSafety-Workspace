// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterCraneHoistAssetCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RegisterCraneHoistAssetCommand"/>.</summary>
public sealed class RegisterCraneHoistAssetCommandValidator : AbstractValidator<Commands.RegisterCraneHoistAssetCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterCraneHoistAssetCommand"/>.</summary>
    public RegisterCraneHoistAssetCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AssetTag).NotEmpty().MaximumLength(400);
        RuleFor(x => x.AssetTitle).NotEmpty().MaximumLength(400);
        RuleFor(x => x.AssetType).NotEmpty();
        RuleFor(x => x.RatedLoadCapacityKg).GreaterThan(0m);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

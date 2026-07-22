// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for FlagObstructionMaterialHandlingStorageCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.FlagObstructionMaterialHandlingStorageCommand"/>.</summary>
public sealed class FlagObstructionMaterialHandlingStorageCommandValidator : AbstractValidator<Commands.FlagObstructionMaterialHandlingStorageCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagObstructionMaterialHandlingStorageCommand"/>.</summary>
    public FlagObstructionMaterialHandlingStorageCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MaterialHandlingStorageId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ObstructionCount).GreaterThanOrEqualTo(0);
    }
}
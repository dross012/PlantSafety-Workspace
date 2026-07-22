// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishMaterialHandlingStorageCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.PublishMaterialHandlingStorageCommand"/>.</summary>
public sealed class PublishMaterialHandlingStorageCommandValidator : AbstractValidator<Commands.PublishMaterialHandlingStorageCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishMaterialHandlingStorageCommand"/>.</summary>
    public PublishMaterialHandlingStorageCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MaterialHandlingStorageId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.ReviewCycleDays).GreaterThanOrEqualTo(0);
    }
}
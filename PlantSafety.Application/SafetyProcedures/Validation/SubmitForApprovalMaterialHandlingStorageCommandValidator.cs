// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for SubmitForApprovalMaterialHandlingStorageCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.SubmitForApprovalMaterialHandlingStorageCommand"/>.</summary>
public sealed class SubmitForApprovalMaterialHandlingStorageCommandValidator : AbstractValidator<Commands.SubmitForApprovalMaterialHandlingStorageCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SubmitForApprovalMaterialHandlingStorageCommand"/>.</summary>
    public SubmitForApprovalMaterialHandlingStorageCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MaterialHandlingStorageId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
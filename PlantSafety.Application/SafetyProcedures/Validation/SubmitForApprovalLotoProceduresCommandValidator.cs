// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for SubmitForApprovalLotoProceduresCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.SubmitForApprovalLotoProceduresCommand"/>.</summary>
public sealed class SubmitForApprovalLotoProceduresCommandValidator : AbstractValidator<Commands.SubmitForApprovalLotoProceduresCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SubmitForApprovalLotoProceduresCommand"/>.</summary>
    public SubmitForApprovalLotoProceduresCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProceduresId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
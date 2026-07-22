// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for FlagPPEAssignmentForReplacementCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.FlagPPEAssignmentForReplacementCommand"/>.</summary>
public sealed class FlagPPEAssignmentForReplacementCommandValidator : AbstractValidator<Commands.FlagPPEAssignmentForReplacementCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagPPEAssignmentForReplacementCommand"/>.</summary>
    public FlagPPEAssignmentForReplacementCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEAssignmentId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetirePPEAssignmentCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.RetirePPEAssignmentCommand"/>.</summary>
public sealed class RetirePPEAssignmentCommandValidator : AbstractValidator<Commands.RetirePPEAssignmentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetirePPEAssignmentCommand"/>.</summary>
    public RetirePPEAssignmentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEAssignmentId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

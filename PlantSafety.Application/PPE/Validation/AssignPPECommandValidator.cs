// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for AssignPPECommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.AssignPPECommand"/>.</summary>
public sealed class AssignPPECommandValidator : AbstractValidator<Commands.AssignPPECommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AssignPPECommand"/>.</summary>
    public AssignPPECommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEAssignmentId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

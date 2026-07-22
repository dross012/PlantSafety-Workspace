// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ArchivePermitWorkflowCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.ArchivePermitWorkflowCommand"/>.</summary>
public sealed class ArchivePermitWorkflowCommandValidator : AbstractValidator<Commands.ArchivePermitWorkflowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ArchivePermitWorkflowCommand"/>.</summary>
    public ArchivePermitWorkflowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitWorkflowId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

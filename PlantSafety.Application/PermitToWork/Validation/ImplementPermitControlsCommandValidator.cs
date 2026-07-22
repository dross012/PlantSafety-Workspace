// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ImplementPermitControlsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.ImplementPermitControlsCommand"/>.</summary>
public sealed class ImplementPermitControlsCommandValidator : AbstractValidator<Commands.ImplementPermitControlsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ImplementPermitControlsCommand"/>.</summary>
    public ImplementPermitControlsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitWorkflowId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ControlsConfirmedBy).NotEmpty().MaximumLength(256);
    }
}

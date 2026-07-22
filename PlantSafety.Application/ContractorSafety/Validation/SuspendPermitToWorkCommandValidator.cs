// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for SuspendPermitToWorkCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.SuspendPermitToWorkCommand"/>.</summary>
public sealed class SuspendPermitToWorkCommandValidator : AbstractValidator<Commands.SuspendPermitToWorkCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SuspendPermitToWorkCommand"/>.</summary>
    public SuspendPermitToWorkCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitToWorkId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
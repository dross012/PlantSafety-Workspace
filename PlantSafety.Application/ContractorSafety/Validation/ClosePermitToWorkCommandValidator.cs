// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ClosePermitToWorkCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.ClosePermitToWorkCommand"/>.</summary>
public sealed class ClosePermitToWorkCommandValidator : AbstractValidator<Commands.ClosePermitToWorkCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ClosePermitToWorkCommand"/>.</summary>
    public ClosePermitToWorkCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitToWorkId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
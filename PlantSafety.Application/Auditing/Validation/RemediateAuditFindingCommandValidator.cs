// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RemediateAuditFindingCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.Validation;

/// <summary>Validates <see cref="Commands.RemediateAuditFindingCommand"/>.</summary>
public sealed class RemediateAuditFindingCommandValidator : AbstractValidator<Commands.RemediateAuditFindingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RemediateAuditFindingCommand"/>.</summary>
    public RemediateAuditFindingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.AuditFindingId).NotEmpty();
        RuleFor(x => x.RemediationNotes).NotEmpty().MaximumLength(2048);
    }
}

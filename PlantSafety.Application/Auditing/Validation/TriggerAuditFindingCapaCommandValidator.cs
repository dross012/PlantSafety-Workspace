// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for TriggerAuditFindingCapaCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.Validation;

/// <summary>Validates <see cref="Commands.TriggerAuditFindingCapaCommand"/>.</summary>
public sealed class TriggerAuditFindingCapaCommandValidator : AbstractValidator<Commands.TriggerAuditFindingCapaCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.TriggerAuditFindingCapaCommand"/>.</summary>
    public TriggerAuditFindingCapaCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.AuditFindingId).NotEmpty();
        RuleFor(x => x.AssignedToUserId).NotEmpty();
        RuleFor(x => x.DueDateUtc).NotEmpty();
    }
}

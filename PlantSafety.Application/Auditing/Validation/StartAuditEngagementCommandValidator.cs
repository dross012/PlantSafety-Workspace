// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for StartAuditEngagementCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.Validation;

/// <summary>Validates <see cref="Commands.StartAuditEngagementCommand"/>.</summary>
public sealed class StartAuditEngagementCommandValidator : AbstractValidator<Commands.StartAuditEngagementCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.StartAuditEngagementCommand"/>.</summary>
    public StartAuditEngagementCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.AuditEngagementId).NotEmpty();
    }
}

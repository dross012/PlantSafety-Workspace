// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for CloseAuditEngagementCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.Validation;

/// <summary>Validates <see cref="Commands.CloseAuditEngagementCommand"/>.</summary>
public sealed class CloseAuditEngagementCommandValidator : AbstractValidator<Commands.CloseAuditEngagementCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CloseAuditEngagementCommand"/>.</summary>
    public CloseAuditEngagementCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.AuditEngagementId).NotEmpty();
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m).When(x => x.ComplianceScore.HasValue);
        RuleFor(x => x.ComplianceRating).NotEmpty().MaximumLength(64);
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordAuditEngagementFindingCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.Validation;

/// <summary>Validates <see cref="Commands.RecordAuditEngagementFindingCommand"/>.</summary>
public sealed class RecordAuditEngagementFindingCommandValidator : AbstractValidator<Commands.RecordAuditEngagementFindingCommand>
{
    private static readonly string[] AllowedSeverities = ["Low", "Medium", "High", "Critical"];

    /// <summary>Defines validation rules for <see cref="Commands.RecordAuditEngagementFindingCommand"/>.</summary>
    public RecordAuditEngagementFindingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.AuditEngagementId).NotEmpty();
        RuleFor(x => x.AuditFindingId).NotEmpty();
        RuleFor(x => x.Severity).NotEmpty().Must(s => AllowedSeverities.Contains(s))
            .WithMessage("Severity must be one of: Low, Medium, High, Critical.");
    }
}

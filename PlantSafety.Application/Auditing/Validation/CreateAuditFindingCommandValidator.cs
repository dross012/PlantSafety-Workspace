// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: FluentValidation validator for CreateAuditFindingCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.Validation;

/// <summary>Validates <see cref="Commands.CreateAuditFindingCommand"/>.</summary>
public sealed class CreateAuditFindingCommandValidator : AbstractValidator<Commands.CreateAuditFindingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateAuditFindingCommand"/>.</summary>
    public CreateAuditFindingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.AuditEngagementId).NotEmpty();
        RuleFor(x => x.FindingTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Severity).NotEmpty();
    }
}
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: FluentValidation validator for RecordConcurrencyConflictCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Auditing.AuditTimeline.Validation;

/// <summary>Validates <see cref="Commands.RecordConcurrencyConflictCommand"/>.</summary>
public sealed class RecordConcurrencyConflictCommandValidator : AbstractValidator<Commands.RecordConcurrencyConflictCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordConcurrencyConflictCommand"/>.</summary>
    public RecordConcurrencyConflictCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.ExpectedETag).NotEmpty().MaximumLength(64);
        RuleFor(x => x.AttemptedOperation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for LogCommunicationEntryCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PublicSafetyCommunication.Validation;

/// <summary>Validates <see cref="Commands.LogCommunicationEntryCommand"/>.</summary>
public sealed class LogCommunicationEntryCommandValidator : AbstractValidator<Commands.LogCommunicationEntryCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.LogCommunicationEntryCommand"/>.</summary>
    public LogCommunicationEntryCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Direction).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Medium).NotEmpty().MaximumLength(32);
        RuleFor(x => x.CounterpartyName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.OccurredAtUtc).NotEmpty();
    }
}

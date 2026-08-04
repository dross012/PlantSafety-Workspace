// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for IngestAccessEventCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.IngestAccessEventCommand"/>.</summary>
public sealed class IngestAccessEventCommandValidator : AbstractValidator<Commands.IngestAccessEventCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.IngestAccessEventCommand"/>.</summary>
    public IngestAccessEventCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccessPointId).NotEmpty();
        RuleFor(x => x.EventType).NotEmpty();
        RuleFor(x => x.Direction).NotEmpty();
        RuleFor(x => x.OccurredAtUtc).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

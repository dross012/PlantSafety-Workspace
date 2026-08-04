// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordRouteObstructionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;

/// <summary>Validates <see cref="Commands.RecordRouteObstructionCommand"/>.</summary>
public sealed class RecordRouteObstructionCommandValidator : AbstractValidator<Commands.RecordRouteObstructionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordRouteObstructionCommand"/>.</summary>
    public RecordRouteObstructionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EvacuationRouteId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Severity).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

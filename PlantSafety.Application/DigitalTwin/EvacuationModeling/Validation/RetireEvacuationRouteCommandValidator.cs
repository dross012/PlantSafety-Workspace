// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RetireEvacuationRouteCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;

/// <summary>Validates <see cref="Commands.RetireEvacuationRouteCommand"/>.</summary>
public sealed class RetireEvacuationRouteCommandValidator : AbstractValidator<Commands.RetireEvacuationRouteCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireEvacuationRouteCommand"/>.</summary>
    public RetireEvacuationRouteCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EvacuationRouteId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

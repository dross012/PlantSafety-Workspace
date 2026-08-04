// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DetectAccessAnomalyCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.DetectAccessAnomalyCommand"/>.</summary>
public sealed class DetectAccessAnomalyCommandValidator : AbstractValidator<Commands.DetectAccessAnomalyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DetectAccessAnomalyCommand"/>.</summary>
    public DetectAccessAnomalyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccessPointId).NotEmpty();
        RuleFor(x => x.AnomalyType).NotEmpty();
        RuleFor(x => x.SeverityScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

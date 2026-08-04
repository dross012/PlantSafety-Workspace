// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for FlagAccessEventAnomalyCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.FlagAccessEventAnomalyCommand"/>.</summary>
public sealed class FlagAccessEventAnomalyCommandValidator : AbstractValidator<Commands.FlagAccessEventAnomalyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagAccessEventAnomalyCommand"/>.</summary>
    public FlagAccessEventAnomalyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccessEventId).NotEmpty();
        RuleFor(x => x.AnomalyType).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

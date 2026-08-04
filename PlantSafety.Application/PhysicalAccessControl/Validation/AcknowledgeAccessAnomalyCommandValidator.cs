// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for AcknowledgeAccessAnomalyCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.AcknowledgeAccessAnomalyCommand"/>.</summary>
public sealed class AcknowledgeAccessAnomalyCommandValidator : AbstractValidator<Commands.AcknowledgeAccessAnomalyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AcknowledgeAccessAnomalyCommand"/>.</summary>
    public AcknowledgeAccessAnomalyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccessAnomalyId).NotEmpty();
        RuleFor(x => x.AcknowledgedByUserId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

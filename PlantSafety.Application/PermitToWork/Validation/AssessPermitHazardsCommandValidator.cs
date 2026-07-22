// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for AssessPermitHazardsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.AssessPermitHazardsCommand"/>.</summary>
public sealed class AssessPermitHazardsCommandValidator : AbstractValidator<Commands.AssessPermitHazardsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AssessPermitHazardsCommand"/>.</summary>
    public AssessPermitHazardsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitWorkflowId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.RequiredControls).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RiskScore).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
    }
}

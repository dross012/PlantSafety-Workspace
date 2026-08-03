// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: FluentValidation validator for EvaluateComplianceCommand. Enforces
//   invariants and normalization boundaries only; never calls repositories or external services
//   (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Application.Compliance.RulesEngine.Validation;

/// <summary>Validates <see cref="Commands.EvaluateComplianceCommand"/>.</summary>
public sealed class EvaluateComplianceCommandValidator : AbstractValidator<Commands.EvaluateComplianceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.EvaluateComplianceCommand"/>.</summary>
    public EvaluateComplianceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TriggeredBy).NotEmpty();
        RuleFor(x => x.TargetEntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetEntityId).NotEmpty();
        RuleForEach(x => x.Categories).Must(ComplianceRuleCategory.IsValid)
            .WithMessage("Category must be one of: Safety, Regulatory, InternalPolicy.")
            .When(x => x.Categories is not null);
        RuleFor(x => x.Facts).NotNull();
    }
}

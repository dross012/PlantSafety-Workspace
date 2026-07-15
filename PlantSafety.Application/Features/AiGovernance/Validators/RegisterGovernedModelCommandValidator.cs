// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: FluentValidation validator for RegisterGovernedModelCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.AiGovernance.Commands.RegisterModel;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.AiGovernance.Validators;

/// <summary>Validates RegisterGovernedModelCommand structural fields.</summary>
public sealed class RegisterGovernedModelCommandValidator : AbstractValidator<RegisterGovernedModelCommand>
{
    private static readonly string[] ValidRiskTiers = ["Low", "Medium", "High", "Critical"];

    /// <summary>Defines validation rules.</summary>
    public RegisterGovernedModelCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.ModelVersion).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RiskTier).NotEmpty().Must(v => ValidRiskTiers.Contains(v))
            .WithMessage("RiskTier must be one of: Low, Medium, High, Critical.");
    }
}

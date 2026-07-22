// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: FluentValidation validator for CreateSafetyPolicyCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.SafetyPolicies.Enums;

namespace PlantSafety.Application.SafetyPolicies.Validation;

/// <summary>Validates <see cref="Commands.CreateSafetyPolicyCommand"/>.</summary>
public sealed class CreateSafetyPolicyCommandValidator : AbstractValidator<Commands.CreateSafetyPolicyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateSafetyPolicyCommand"/>.</summary>
    public CreateSafetyPolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.SafetyPolicyTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(PolicyCategory.IsValid)
            .WithMessage("Category must be a recognized PolicyCategory code.");
    }
}
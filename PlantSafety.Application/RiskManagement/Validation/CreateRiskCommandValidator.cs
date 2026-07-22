// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- validates the real hazard-identification fields (location, hazard source).
// FILE DESCRIPTION: FluentValidation validator for CreateRiskCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 2

using FluentValidation;

namespace PlantSafety.Application.RiskManagement.Validation;

/// <summary>Validates <see cref="Commands.CreateRiskCommand"/>.</summary>
public sealed class CreateRiskCommandValidator : AbstractValidator<Commands.CreateRiskCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateRiskCommand"/>.</summary>
    public CreateRiskCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.RiskTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(256);
        RuleFor(x => x.HazardSource).NotEmpty().MaximumLength(128);
    }
}

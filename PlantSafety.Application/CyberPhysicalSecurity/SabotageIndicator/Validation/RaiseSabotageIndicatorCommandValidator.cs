// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RaiseSabotageIndicatorCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Validation;

/// <summary>Validates <see cref="Commands.RaiseSabotageIndicatorCommand"/>.</summary>
public sealed class RaiseSabotageIndicatorCommandValidator : AbstractValidator<Commands.RaiseSabotageIndicatorCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RaiseSabotageIndicatorCommand"/>.</summary>
    public RaiseSabotageIndicatorCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Source).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Severity).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

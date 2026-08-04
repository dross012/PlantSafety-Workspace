// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for LogCommunityConcernCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CommunitySafety.Validation;

/// <summary>Validates <see cref="Commands.LogCommunityConcernCommand"/>.</summary>
public sealed class LogCommunityConcernCommandValidator : AbstractValidator<Commands.LogCommunityConcernCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.LogCommunityConcernCommand"/>.</summary>
    public LogCommunityConcernCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Severity).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FacilityArea).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

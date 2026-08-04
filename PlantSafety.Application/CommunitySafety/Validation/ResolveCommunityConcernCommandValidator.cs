// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for ResolveCommunityConcernCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CommunitySafety.Validation;

/// <summary>Validates <see cref="Commands.ResolveCommunityConcernCommand"/>.</summary>
public sealed class ResolveCommunityConcernCommandValidator : AbstractValidator<Commands.ResolveCommunityConcernCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ResolveCommunityConcernCommand"/>.</summary>
    public ResolveCommunityConcernCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CommunityConcernId).NotEmpty();
        RuleFor(x => x.ResolutionNotes).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

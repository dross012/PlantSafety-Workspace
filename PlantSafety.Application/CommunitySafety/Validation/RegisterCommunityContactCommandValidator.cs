// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterCommunityContactCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CommunitySafety.Validation;

/// <summary>Validates <see cref="Commands.RegisterCommunityContactCommand"/>.</summary>
public sealed class RegisterCommunityContactCommandValidator : AbstractValidator<Commands.RegisterCommunityContactCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterCommunityContactCommand"/>.</summary>
    public RegisterCommunityContactCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactType).NotEmpty();
        RuleFor(x => x.CommunityArea).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Organization).MaximumLength(200).When(x => x.Organization is not null);
        RuleFor(x => x.Phone).MaximumLength(32).When(x => x.Phone is not null);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

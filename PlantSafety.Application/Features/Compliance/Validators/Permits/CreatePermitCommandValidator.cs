// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreatePermitCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.Compliance.Validators.Permits;

/// <summary>Validates <see cref="CreatePermitCommand"/> structural fields.</summary>
public sealed class CreatePermitCommandValidator : AbstractValidator<CreatePermitCommand>
{
    private static readonly string[] ValidPermitTypes =
        ["HotWork", "ColdWork", "ConfinedSpace", "Electrical", "LockoutTagout"];

    /// <summary>Defines validation rules.</summary>
    public CreatePermitCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<CreatePermitCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.CorrelationId).IsValidId<CreatePermitCommand, Guid>(x => x.CorrelationId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.WorkLeadId).NotEmpty();

        RuleFor(x => x.PermitType)
            .Must(t => ValidPermitTypes.Contains(t))
            .WithMessage($"PermitType must be one of: {string.Join(", ", ValidPermitTypes)}");

        RuleFor(x => x.ValidFromUtc).MustBeUtc();
        RuleFor(x => x.ValidToUtc).MustBeUtc();

        RuleFor(x => x).Must(x => x.ValidFromUtc < x.ValidToUtc)
            .WithMessage("ValidFromUtc must be before ValidToUtc.");
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateMocCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.Compliance.Commands.ManagementOfChange.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.Compliance.Validators.ManagementOfChange;

/// <summary>Validates <see cref="CreateMocCommand"/> structural fields.</summary>
public sealed class CreateMocCommandValidator : AbstractValidator<CreateMocCommand>
{
    private static readonly string[] ValidChangeTypes = ["Permanent", "Temporary", "Emergency"];
    private static readonly string[] ValidRiskLevels = ["Low", "Medium", "High", "Critical"];

    /// <summary>Defines validation rules.</summary>
    public CreateMocCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<CreateMocCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.CorrelationId).IsValidId<CreateMocCommand, Guid>(x => x.CorrelationId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.InitiatorId).NotEmpty();

        RuleFor(x => x.ChangeType)
            .Must(t => ValidChangeTypes.Contains(t))
            .WithMessage($"ChangeType must be one of: {string.Join(", ", ValidChangeTypes)}");

        RuleFor(x => x.RiskLevel)
            .Must(l => ValidRiskLevels.Contains(l))
            .WithMessage($"RiskLevel must be one of: {string.Join(", ", ValidRiskLevels)}");

        RuleFor(x => x.ProposedStartUtc).MustBeUtc();
        RuleFor(x => x.ProposedEndUtc).MustBeUtcWhenProvided();

        RuleFor(x => x).Must(x =>
            x.ProposedEndUtc is null || x.ProposedStartUtc < x.ProposedEndUtc)
            .WithMessage("ProposedStartUtc must be before ProposedEndUtc when both are provided.");
    }
}

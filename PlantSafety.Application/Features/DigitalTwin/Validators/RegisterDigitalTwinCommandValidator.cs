// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: FluentValidation validator for RegisterDigitalTwinCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.DigitalTwin.Commands.RegisterTwin;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.DigitalTwin.Validators;

/// <summary>Validates <see cref="RegisterDigitalTwinCommand"/> structural fields.</summary>
public sealed class RegisterDigitalTwinCommandValidator : AbstractValidator<RegisterDigitalTwinCommand>
{
    /// <summary>Defines validation rules.</summary>
    public RegisterDigitalTwinCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.AssetName).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.AssetType).NotEmpty().MaximumLength(CommonValidationRules.MaxReferenceLength);
    }
}

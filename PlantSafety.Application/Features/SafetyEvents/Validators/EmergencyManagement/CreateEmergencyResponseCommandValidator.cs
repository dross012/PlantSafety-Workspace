// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateEmergencyResponseCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.EmergencyManagement.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.EmergencyManagement;

/// <summary>Validates <see cref="CreateEmergencyResponseCommand"/> structural fields.</summary>
public sealed class CreateEmergencyResponseCommandValidator : AbstractValidator<CreateEmergencyResponseCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CreateEmergencyResponseCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<CreateEmergencyResponseCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.CorrelationId).IsValidId<CreateEmergencyResponseCommand, Guid>(x => x.CorrelationId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.ResponsePlanName).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.ActivatedAtUtc).MustBeUtc().MustNotBeInFuture();
    }
}

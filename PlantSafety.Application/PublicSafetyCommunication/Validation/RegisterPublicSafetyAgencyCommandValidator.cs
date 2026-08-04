// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterPublicSafetyAgencyCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PublicSafetyCommunication.Validation;

/// <summary>Validates <see cref="Commands.RegisterPublicSafetyAgencyCommand"/>.</summary>
public sealed class RegisterPublicSafetyAgencyCommandValidator : AbstractValidator<Commands.RegisterPublicSafetyAgencyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterPublicSafetyAgencyCommand"/>.</summary>
    public RegisterPublicSafetyAgencyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AgencyName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.AgencyType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.JurisdictionDescription).NotEmpty().MaximumLength(400);
        RuleFor(x => x.PrimaryPhoneNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PrimaryEmail).MaximumLength(320).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.PrimaryEmail));
    }
}

// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: FluentValidation validator for
//   RegisterPartnerIdentityMappingCommand. Enforces invariants and
//   normalization boundaries only; never calls repositories or external
//   services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Identity.Commands;

namespace PlantSafety.Application.Identity.Validators;

/// <summary>Validates <see cref="RegisterPartnerIdentityMappingCommand"/>.</summary>
public sealed class RegisterPartnerIdentityMappingCommandValidator : AbstractValidator<RegisterPartnerIdentityMappingCommand>
{
    /// <summary>Defines validation rules for <see cref="RegisterPartnerIdentityMappingCommand"/>.</summary>
    public RegisterPartnerIdentityMappingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.InternalUserId).NotEmpty();
        RuleFor(x => x.PartnerIssuer).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PartnerExternalId).NotEmpty().MaximumLength(500);
    }
}

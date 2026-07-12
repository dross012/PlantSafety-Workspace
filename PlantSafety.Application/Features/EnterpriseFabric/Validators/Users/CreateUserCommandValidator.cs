// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateUserCommand.
//   Email format is validated structurally only; no external lookup is performed.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.Users.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.EnterpriseFabric.Validators.Users;

/// <summary>Validates <see cref="CreateUserCommand"/> structural fields.</summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Roles).NotEmpty().WithMessage("At least one role must be assigned.");
        RuleForEach(x => x.Roles).NotEmpty().MaximumLength(100);

        RuleFor(x => x.SiteIds).NotNull();
        RuleForEach(x => x.SiteIds).NotEmpty();
    }
}

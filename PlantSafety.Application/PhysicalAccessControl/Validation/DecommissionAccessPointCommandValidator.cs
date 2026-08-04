// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DecommissionAccessPointCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.DecommissionAccessPointCommand"/>.</summary>
public sealed class DecommissionAccessPointCommandValidator : AbstractValidator<Commands.DecommissionAccessPointCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DecommissionAccessPointCommand"/>.</summary>
    public DecommissionAccessPointCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccessPointId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

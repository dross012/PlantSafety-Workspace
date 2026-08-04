// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterAccessPointCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.RegisterAccessPointCommand"/>.</summary>
public sealed class RegisterAccessPointCommandValidator : AbstractValidator<Commands.RegisterAccessPointCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterAccessPointCommand"/>.</summary>
    public RegisterAccessPointCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccessPointCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DeviceType).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

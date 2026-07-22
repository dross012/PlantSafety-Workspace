// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for DeliverOrientationSiteOrientationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.DeliverOrientationSiteOrientationCommand"/>.</summary>
public sealed class DeliverOrientationSiteOrientationCommandValidator : AbstractValidator<Commands.DeliverOrientationSiteOrientationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeliverOrientationSiteOrientationCommand"/>.</summary>
    public DeliverOrientationSiteOrientationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteOrientationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.AttendeeCount).GreaterThanOrEqualTo(0);
    }
}
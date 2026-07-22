// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ApprovePreQualificationVettingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.ApprovePreQualificationVettingCommand"/>.</summary>
public sealed class ApprovePreQualificationVettingCommandValidator : AbstractValidator<Commands.ApprovePreQualificationVettingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ApprovePreQualificationVettingCommand"/>.</summary>
    public ApprovePreQualificationVettingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PreQualificationVettingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.EmrScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.TrirScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.DeficiencyCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValidityPeriodDays).GreaterThanOrEqualTo(0);
    }
}
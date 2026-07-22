// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for BeginReviewPreQualificationVettingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.BeginReviewPreQualificationVettingCommand"/>.</summary>
public sealed class BeginReviewPreQualificationVettingCommandValidator : AbstractValidator<Commands.BeginReviewPreQualificationVettingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginReviewPreQualificationVettingCommand"/>.</summary>
    public BeginReviewPreQualificationVettingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PreQualificationVettingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ReviewedByUserId).NotEmpty();
    }
}
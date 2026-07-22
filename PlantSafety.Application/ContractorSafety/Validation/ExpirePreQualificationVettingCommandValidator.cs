// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ExpirePreQualificationVettingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.ExpirePreQualificationVettingCommand"/>.</summary>
public sealed class ExpirePreQualificationVettingCommandValidator : AbstractValidator<Commands.ExpirePreQualificationVettingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ExpirePreQualificationVettingCommand"/>.</summary>
    public ExpirePreQualificationVettingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PreQualificationVettingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
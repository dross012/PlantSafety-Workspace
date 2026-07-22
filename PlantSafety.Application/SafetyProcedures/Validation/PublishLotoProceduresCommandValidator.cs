// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishLotoProceduresCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.PublishLotoProceduresCommand"/>.</summary>
public sealed class PublishLotoProceduresCommandValidator : AbstractValidator<Commands.PublishLotoProceduresCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishLotoProceduresCommand"/>.</summary>
    public PublishLotoProceduresCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProceduresId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.AuthorizedPersonnelCount).GreaterThanOrEqualTo(0);
    }
}
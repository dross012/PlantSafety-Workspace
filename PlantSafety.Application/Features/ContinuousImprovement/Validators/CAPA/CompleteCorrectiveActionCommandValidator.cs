// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: FluentValidation validator for CompleteCorrectiveActionCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Complete;
using PlantSafety.Application.Validation.Extensions;

namespace PlantSafety.Application.Features.ContinuousImprovement.Validators.CAPA;

/// <summary>Validates <see cref="CompleteCorrectiveActionCommand"/> structural fields.</summary>
public sealed class CompleteCorrectiveActionCommandValidator : AbstractValidator<CompleteCorrectiveActionCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CompleteCorrectiveActionCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.CaId).NotEmpty();
    }
}

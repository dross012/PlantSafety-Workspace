// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishHazardCommunicationRightToKnowCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.PublishHazardCommunicationRightToKnowCommand"/>.</summary>
public sealed class PublishHazardCommunicationRightToKnowCommandValidator : AbstractValidator<Commands.PublishHazardCommunicationRightToKnowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishHazardCommunicationRightToKnowCommand"/>.</summary>
    public PublishHazardCommunicationRightToKnowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardCommunicationRightToKnowId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.LabeledItemCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReviewCycleDays).GreaterThanOrEqualTo(0);
    }
}
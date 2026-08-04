// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordCommunityMeetingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CommunitySafety.Validation;

/// <summary>Validates <see cref="Commands.RecordCommunityMeetingCommand"/>.</summary>
public sealed class RecordCommunityMeetingCommandValidator : AbstractValidator<Commands.RecordCommunityMeetingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordCommunityMeetingCommand"/>.</summary>
    public RecordCommunityMeetingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.MeetingTitle).NotEmpty().MaximumLength(300);
        RuleFor(x => x.MeetingDateUtc).NotEmpty();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.AttendeeCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TopicsSummary).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.RecordedByUserId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}

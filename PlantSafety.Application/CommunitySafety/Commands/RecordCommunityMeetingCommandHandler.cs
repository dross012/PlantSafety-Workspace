// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordCommunityMeetingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Interfaces;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Aggregates.CommunityMeetingRecord;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Handles <see cref="RecordCommunityMeetingCommand"/>.</summary>
public sealed class RecordCommunityMeetingCommandHandler : IRequestHandler<RecordCommunityMeetingCommand, Result<CommunityMeetingRecordActionResult>>
{
    private readonly ICommunityMeetingRecordRepository _repository;
    private readonly ILogger<RecordCommunityMeetingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordCommunityMeetingCommandHandler(ICommunityMeetingRecordRepository repository, ILogger<RecordCommunityMeetingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityMeetingRecordActionResult>> Handle(RecordCommunityMeetingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var recordedBy = UserId.FromGuid(request.RecordedByUserId);

            var aggregate = Agg.Record(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                meetingTitle: request.MeetingTitle,
                meetingDateUtc: new UtcTimestamp(request.MeetingDateUtc),
                location: request.Location,
                attendeeCount: request.AttendeeCount,
                topicsSummary: request.TopicsSummary,
                recordedByUserId: recordedBy,
                actionItems: request.ActionItems);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CommunityMeetingRecordActionResult>.Success(new CommunityMeetingRecordActionResult(
                aggregate.CommunityMeetingRecordId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record CommunityMeetingRecord for Tenant {TenantId}", request.TenantId);

            return Result<CommunityMeetingRecordActionResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityMeetingRecord.RecordFailed",
                "Unable to record the CommunityMeetingRecord at this time.",
                Guid.NewGuid()));
        }
    }
}

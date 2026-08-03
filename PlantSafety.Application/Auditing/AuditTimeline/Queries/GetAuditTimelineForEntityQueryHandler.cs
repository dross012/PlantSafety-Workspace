// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetAuditTimelineForEntityQuery. Gathers raw entries from all four
//   sources (event timeline, state changes, concurrency conflicts, scheduling decisions) and hands
//   them to the pure Domain-layer AuditTimelineCompositionService for deterministic chronological
//   composition -- this handler performs no ordering logic itself. Additive-only evolution.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;
using PlantSafety.Domain.Auditing.AuditTimeline.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditTimeline.Mapping;
using PlantSafety.Application.Auditing.AuditTimeline.Models;
using PlantSafety.Application.Auditing.AuditTimeline.Repositories;

namespace PlantSafety.Application.Auditing.AuditTimeline.Queries;

/// <summary>Handles <see cref="GetAuditTimelineForEntityQuery"/>.</summary>
public sealed class GetAuditTimelineForEntityQueryHandler : IRequestHandler<GetAuditTimelineForEntityQuery, Result<AuditTimelineDto>>
{
    private readonly IAuditTimelineEventSourceRepository _eventSource;
    private readonly IAuditTimelineStateChangeRepository _stateChangeSource;
    private readonly IConcurrencyConflictRepository _concurrencyConflictSource;
    private readonly ISchedulingDecisionHistoryProvider _schedulingDecisionSource;
    private readonly AuditTimelineCompositionService _compositionService;
    private readonly ILogger<GetAuditTimelineForEntityQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditTimelineForEntityQueryHandler(
        IAuditTimelineEventSourceRepository eventSource,
        IAuditTimelineStateChangeRepository stateChangeSource,
        IConcurrencyConflictRepository concurrencyConflictSource,
        ISchedulingDecisionHistoryProvider schedulingDecisionSource,
        AuditTimelineCompositionService compositionService,
        ILogger<GetAuditTimelineForEntityQueryHandler> logger)
    {
        _eventSource = eventSource;
        _stateChangeSource = stateChangeSource;
        _concurrencyConflictSource = concurrencyConflictSource;
        _schedulingDecisionSource = schedulingDecisionSource;
        _compositionService = compositionService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditTimelineDto>> Handle(GetAuditTimelineForEntityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var eventEntries = await _eventSource.GetEventEntriesAsync(request.TenantId, request.EntityType, request.EntityId, cancellationToken);
            var stateChangeEntries = await _stateChangeSource.GetStateChangeEntriesAsync(request.TenantId, request.EntityType, request.EntityId, cancellationToken);
            var schedulingEntries = await _schedulingDecisionSource.GetSchedulingDecisionEntriesAsync(request.TenantId, request.EntityType, request.EntityId, cancellationToken);

            var conflictRecords = await _concurrencyConflictSource.GetForEntityAsync(request.TenantId, request.EntityType, request.EntityId, cancellationToken);
            var conflictEntries = conflictRecords.Select(AuditTimelineMapper.ToTimelineEntry).ToList();

            var composed = _compositionService.Compose(eventEntries, stateChangeEntries, conflictEntries, schedulingEntries);

            var dto = new AuditTimelineDto
            {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Entries = composed.Select(AuditTimelineMapper.ToDto).ToList(),
                ComposedAtUtc = UtcTimestamp.Now().Value,
            };

            return Result<AuditTimelineDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reconstruct AuditTimeline for {EntityType} {EntityId} Tenant {TenantId}",
                request.EntityType, request.EntityId, request.TenantId);

            return Result<AuditTimelineDto>.Failure(new ApplicationError(
                "Auditing.AuditTimeline.ReconstructionFailed",
                "Unable to reconstruct the audit timeline for the requested entity at this time.",
                Guid.NewGuid()));
        }
    }
}

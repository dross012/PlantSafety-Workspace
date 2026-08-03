// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: MediatR handler for GetIncidentTimelineQuery.
//   Reads from IIncidentTimelineReadRepository (read-optimised stored proc against
//   [SafetyEvents].[IncidentTimelineEntry], populated by the aggregate's lifecycle methods).
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Timeline;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="GetIncidentTimelineQuery"/>.
/// Returns a paginated, chronologically-ordered <see cref="IncidentTimelineEntryDto"/> list.
/// </summary>
public sealed class GetIncidentTimelineQueryHandler
    : IQueryHandler<GetIncidentTimelineQuery, PaginatedResult<IncidentTimelineEntryDto>>
{
    private readonly IIncidentTimelineReadRepository _timelineReadRepository;
    private readonly ILogger<GetIncidentTimelineQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentTimelineQueryHandler(
        IIncidentTimelineReadRepository timelineReadRepository,
        ILogger<GetIncidentTimelineQueryHandler> logger)
    {
        _timelineReadRepository = timelineReadRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<IncidentTimelineEntryDto>> Handle(
        GetIncidentTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (readModels, totalCount) = await _timelineReadRepository.ListAsync(
            tenantId: request.TenantId,
            incidentId: request.IncidentId,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Timeline for Incident {IncidentId} Tenant {TenantId}: {Count} entries",
            request.IncidentId, request.TenantId, totalCount);

        var items = readModels.Select(r => new IncidentTimelineEntryDto
        {
            EntryId = r.EntryId,
            TimestampUtc = r.TimestampUtc,
            Actor = r.Actor,
            EventType = r.EventType,
            Severity = r.Severity,
            Description = r.Description,
        }).ToList();

        return new PaginatedResult<IncidentTimelineEntryDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }
}

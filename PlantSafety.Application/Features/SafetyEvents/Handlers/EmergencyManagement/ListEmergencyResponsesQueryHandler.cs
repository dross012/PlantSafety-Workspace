// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: MediatR handler for ListEmergencyResponsesQuery.
//   Returns a canonically paginated result set per §XVIII.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.EmergencyManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.EmergencyManagement.List;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;

/// <summary>
/// Handles <see cref="ListEmergencyResponsesQuery"/>.
/// Returns paginated <see cref="EmergencyResponseDto"/> records.
/// </summary>
public sealed class ListEmergencyResponsesQueryHandler
    : IQueryHandler<ListEmergencyResponsesQuery, PaginatedResult<EmergencyResponseDto>>
{
    private readonly IEmergencyResponseRepository _repository;
    private readonly ILogger<ListEmergencyResponsesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListEmergencyResponsesQueryHandler(
        IEmergencyResponseRepository repository,
        ILogger<ListEmergencyResponsesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<EmergencyResponseDto>> Handle(
        ListEmergencyResponsesQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();
        var orderBy = request.OrderBy.SafeOrderBy("ActivatedAtUtc");
        var sortDirection = request.SortDirection.SafeSortDirection();

        var (readModels, totalCount) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            status: request.Status,
            orderBy: orderBy,
            sortDirection: sortDirection,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = readModels.Select(Map).ToList();

        _logger.LogDebug(
            "Listed {Count} EmergencyResponses for Tenant {TenantId} CorrelationId {CorrelationId}",
            items.Count, request.TenantId, request.CorrelationId);

        return new PaginatedResult<EmergencyResponseDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }

    /// <summary>Explicit, deterministic, one-way mapping from the read model to the DTO.</summary>
    private static EmergencyResponseDto Map(EmergencyResponseReadModel readModel) => new()
    {
        EmergencyResponseId = readModel.ResponseId,
        TenantId = readModel.TenantId,
        SiteId = readModel.SiteId,
        LinkedCrisisEventId = readModel.LinkedCrisisEventId,
        ResponsePlanName = readModel.ResponsePlanName,
        ActivatedAtUtc = readModel.ActivatedAtUtc,
        Status = readModel.Status,
        CoordinatorId = readModel.CoordinatorId,
        StandDownAtUtc = readModel.StandDownAtUtc,
        LastModifiedAtUtc = readModel.LastModifiedAtUtc,
        ETag = readModel.ETag
    };
}

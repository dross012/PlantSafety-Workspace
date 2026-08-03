// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Infrastructure
//  File:           PredictiveMaintenanceDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PredictiveMaintenance read-model. All DB access via
//                  stored procedures in the [Operational] schema.
//                  Phase 4 full build-out (2026-07-30): additionally implements the new
//                  Application-layer by-id read interface
//                  (PlantSafety.Application.Operational.PredictiveMaintenance.Repositories
//                  .IPredictiveMaintenanceAlertRepository), backing
//                  GetPredictiveMaintenanceAlertByIdQuery via usp_PredictiveMaintenanceAlert_GetById.
//                  The pre-existing date-range list method (GetPredictiveMaintenanceAsync, backing
//                  IPredictiveMaintenanceRepository/GetPredictiveMaintenanceQuery) is left
//                  completely untouched. Distinct from
//                  PredictiveMaintenanceAlertDapperRepository.cs (the new write-side aggregate
//                  repository implementing the Domain-layer IPredictiveMaintenanceAlertRepository).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

namespace PlantSafety.Infrastructure.Operational.PredictiveMaintenance.Persistence;

/// <summary>Dapper-backed repository for <see cref="PredictiveMaintenanceItemDto"/> queries.</summary>
public sealed class PredictiveMaintenanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PredictiveMaintenanceDapperRepository> logger)
    : IPredictiveMaintenanceRepository,
      PlantSafety.Application.Operational.PredictiveMaintenance.Repositories.IPredictiveMaintenanceAlertRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PredictiveMaintenanceItemDto>> GetPredictiveMaintenanceAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPredictiveMaintenanceAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PredictiveMaintenanceItemDto>(
            "[Operational].[usp_PredictiveMaintenance_List]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PredictiveMaintenanceAlertItemDto?> GetByIdAsync(Guid predictiveMaintenanceAlertId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId} Tenant {TenantId}", predictiveMaintenanceAlertId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PredictiveMaintenanceAlertId", predictiveMaintenanceAlertId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PredictiveMaintenanceAlertItemDto>(
            "[Operational].[usp_PredictiveMaintenanceAlert_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<PredictiveMaintenanceAlertItemDto> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId,
        Guid? equipmentId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "GetPagedAsync for PredictiveMaintenanceAlert Tenant {TenantId} Equipment {EquipmentId} ActiveOnly {ActiveOnly}",
            tenantId, equipmentId, activeOnly);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("EquipmentId", equipmentId)
            .AddBool("ActiveOnly", activeOnly)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<PredictiveMaintenanceAlertPagedRow>(
            "[Operational].[usp_PredictiveMaintenanceAlert_List]", parameters, cancellationToken);

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

        var items = rows.Select(r => new PredictiveMaintenanceAlertItemDto
        {
            PredictiveMaintenanceAlertId = r.PredictiveMaintenanceAlertId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            Status = r.Status,
            EquipmentId = r.EquipmentId,
            PredictedFailureType = r.PredictedFailureType,
            PredictedAtUtc = r.PredictedAtUtc,
            ReviewedBy = r.ReviewedBy,
            ReviewedAtUtc = r.ReviewedAtUtc,
            ActionScheduledBy = r.ActionScheduledBy,
            WorkOrderReference = r.WorkOrderReference,
            ActionScheduledAtUtc = r.ActionScheduledAtUtc,
            ResolvedBy = r.ResolvedBy,
            ResolutionNotes = r.ResolutionNotes,
            ResolvedAtUtc = r.ResolvedAtUtc,
            DismissedBy = r.DismissedBy,
            DismissedReason = r.DismissedReason,
            DismissedAtUtc = r.DismissedAtUtc,
            ClosedBy = r.ClosedBy,
            ClosedAtUtc = r.ClosedAtUtc,
            CreatedAtUtc = r.CreatedAtUtc,
            CreatedBy = r.CreatedBy,
        }).ToList();

        return (items, totalCount);
    }

    /// <summary>Flat row shape returned by <c>usp_PredictiveMaintenanceAlert_List</c>, carrying the windowed TotalCount alongside every page row.</summary>
    private sealed record PredictiveMaintenanceAlertPagedRow
    {
        public required Guid PredictiveMaintenanceAlertId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid EquipmentId { get; init; }
        public required string PredictedFailureType { get; init; }
        public required DateTime PredictedAtUtc { get; init; }
        public Guid? ReviewedBy { get; init; }
        public DateTime? ReviewedAtUtc { get; init; }
        public Guid? ActionScheduledBy { get; init; }
        public string? WorkOrderReference { get; init; }
        public DateTime? ActionScheduledAtUtc { get; init; }
        public Guid? ResolvedBy { get; init; }
        public string? ResolutionNotes { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public Guid? DismissedBy { get; init; }
        public string? DismissedReason { get; init; }
        public DateTime? DismissedAtUtc { get; init; }
        public Guid? ClosedBy { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public int TotalCount { get; init; }
    }
}

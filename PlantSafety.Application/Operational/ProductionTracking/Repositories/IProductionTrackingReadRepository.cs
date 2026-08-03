// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           IProductionTrackingReadRepository.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the ProductionTracking module's full query
//                  surface (13 queries backing v1/production-tracking/*). Implemented in the
//                  Infrastructure layer (Dapper). Distinct from
//                  PlantSafety.Domain.Operational.ProductionTracking.Interfaces
//                  .IProductionLineRepository/IProductionRunRepository (the write-side aggregate
//                  repositories). Mirrors the CQRS read/write repository separation precedent
//                  already used by Operational.WorkOrders. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.ProductionTracking.Models;

namespace PlantSafety.Application.Operational.ProductionTracking.Repositories;

/// <summary>Read-only repository for the ProductionTracking module's query surface. Implemented in the Infrastructure layer.</summary>
public interface IProductionTrackingReadRepository
{
    /// <summary>Returns the raw KPI inputs for every currently active (Running/Paused) run, for the Throughput/Bottleneck summary cards and the plant-wide KPI dashboard.</summary>
    Task<IReadOnlyList<ProductionRunKpiInputDto>> GetActiveRunKpiInputsAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns a paginated page of active production runs.</summary>
    Task<(IReadOnlyList<ProductionRunRowDto> Items, int TotalCount)> ListActiveRunsAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the Run Summary panel header for a single run, or null if not found.</summary>
    Task<RunSummaryDto?> GetRunSummaryAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken);

    /// <summary>Returns the Run Metadata panel for a single run, or null if not found.</summary>
    Task<RunMetadataDto?> GetRunMetadataAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken);

    /// <summary>Returns the chronological timeline for a single run.</summary>
    Task<(IReadOnlyList<TimelineEventDto> Items, int TotalCount)> GetRunTimelineAsync(
        Guid tenantId, Guid runId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the batches produced by a single run.</summary>
    Task<(IReadOnlyList<BatchRowDto> Items, int TotalCount)> GetRunBatchesAsync(
        Guid tenantId, Guid runId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the raw KPI inputs for a single run, or null if not found.</summary>
    Task<ProductionRunKpiInputDto?> GetRunKpiInputAsync(Guid tenantId, Guid runId, CancellationToken cancellationToken);

    /// <summary>Returns the event log for a single run.</summary>
    Task<(IReadOnlyList<EventRowDto> Items, int TotalCount)> GetRunEventsAsync(
        Guid tenantId, Guid runId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns a paginated page of batches across all runs.</summary>
    Task<(IReadOnlyList<BatchRowDto> Items, int TotalCount)> ListBatchesAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the plant-wide chronological production timeline.</summary>
    Task<(IReadOnlyList<TimelineEventDto> Items, int TotalCount)> GetProductionTimelineAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>Returns the searchable/filterable production event history log.</summary>
    Task<(IReadOnlyList<EventRowDto> Items, int TotalCount)> GetEventHistoryAsync(
        Guid tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

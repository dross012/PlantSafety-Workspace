// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           ProductionTrackingQueries.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Every query record backing the ProductionTracking UI (v1/production-tracking/*):
//                  Dashboard (throughput/bottlenecks/active-runs), Run Detail (summary/metadata/
//                  timeline/batches/kpis/events), Batch Tracking, KPI Dashboard, Production Timeline,
//                  and Event History -- 13 endpoints total, the largest scope of the 9 UI gaps (see
//                  docs/Phase5/ui-gaps-scoping.md "ProductionTracking" section). Consolidated into
//                  one file per the sibling WorkOrderQueries.cs precedent of grouping closely-related
//                  small record types. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.ProductionTracking.Models;

namespace PlantSafety.Application.Operational.ProductionTracking.Queries;

/// <summary>Retrieves the Throughput Summary card.</summary>
public sealed record GetThroughputSummaryQuery(Guid TenantId) : IRequest<Result<ThroughputSummaryDto>>;

/// <summary>Retrieves the Bottleneck Summary card. Reuse point: composed via the real Observability.Telemetry.TelemetryAnomalyDetector.</summary>
public sealed record GetBottleneckSummaryQuery(Guid TenantId) : IRequest<Result<BottleneckSummaryDto>>;

/// <summary>Retrieves a paginated page of active production runs.</summary>
public sealed record ListActiveProductionRunsQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<ProductionRunRowDto>>>;

/// <summary>Retrieves the Run Summary panel for a single run.</summary>
public sealed record GetRunSummaryQuery(Guid TenantId, Guid RunId) : IRequest<Result<RunSummaryDto>>;

/// <summary>Retrieves the Metadata panel for a single run.</summary>
public sealed record GetRunMetadataQuery(Guid TenantId, Guid RunId) : IRequest<Result<RunMetadataDto>>;

/// <summary>Retrieves the chronological timeline for a single run.</summary>
public sealed record GetRunTimelineQuery(Guid TenantId, Guid RunId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<TimelineEventDto>>>;

/// <summary>Retrieves the batches produced by a single run.</summary>
public sealed record GetRunBatchesQuery(Guid TenantId, Guid RunId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<BatchRowDto>>>;

/// <summary>Retrieves the KPI tiles for a single run. Reuse point: composed via the real Analytics.Dashboard.Kpi.ProductionKpiEngine.</summary>
public sealed record GetRunKpisQuery(Guid TenantId, Guid RunId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<KpiTileDto>>>;

/// <summary>Retrieves the event log for a single run.</summary>
public sealed record GetRunEventsQuery(Guid TenantId, Guid RunId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<EventRowDto>>>;

/// <summary>Retrieves a paginated page of batches across all runs.</summary>
public sealed record ListProductionBatchesQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<BatchRowDto>>>;

/// <summary>Retrieves plant-wide KPI tiles for the KPI Dashboard page. Reuse point: composed via the real Analytics.Dashboard.Kpi.ProductionKpiEngine.</summary>
public sealed record GetPlantKpisQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<KpiTileDto>>>;

/// <summary>Retrieves the plant-wide chronological production timeline.</summary>
public sealed record GetProductionTimelineQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<TimelineEventDto>>>;

/// <summary>Retrieves the searchable/filterable production event history log.</summary>
public sealed record GetEventHistoryQuery(Guid TenantId, int PageNumber, int PageSize)
    : IRequest<Result<PaginatedResult<EventRowDto>>>;

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.Rollup
//  Layer:          Application
//  File:           SchedulingRollupQueries.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Every query record backing the Scheduling UI's "Job"/calendar/resource-summary
//                  surface (v1/scheduling/jobs*, v1/scheduling/resources/summary,
//                  v1/scheduling/calendar). Plain MediatR IRequest<Result<T>> records. Every
//                  handler fans out to the real Operational.WorkOrders and Scheduling.PMScheduling
//                  query surfaces via ISender rather than a second, duplicated "Job" domain
//                  concept -- see docs/Phase5/ui-gaps-scoping.md "Scheduling" section. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.Rollup.Models;

namespace PlantSafety.Application.Scheduling.Rollup.Queries;

/// <summary>Retrieves the Job Summary card counts, rolled up across WorkOrders + PMScheduling.</summary>
public sealed record GetJobSummaryQuery(Guid TenantId) : IRequest<Result<JobSummaryDto>>;

/// <summary>Retrieves the Resource Summary card counts, rolled up from WorkOrders' technician workload.</summary>
public sealed record GetResourceSummaryQuery(Guid TenantId) : IRequest<Result<ResourceSummaryDto>>;

/// <summary>Retrieves a paginated page of upcoming jobs for the dashboard table, merged from WorkOrders + PMScheduling.</summary>
public sealed record ListUpcomingJobsQuery(
    Guid TenantId,
    int PageNumber,
    int PageSize,
    string? StatusFilter) : IRequest<Result<PaginatedResult<UpcomingJobDto>>>;

/// <summary>Retrieves the full detail for a single job, resolved as a WorkOrder first, then a PMScheduling record.</summary>
public sealed record GetJobDetailQuery(Guid TenantId, Guid JobId) : IRequest<Result<JobDetailDto>>;

/// <summary>Retrieves the ordered timeline steps for a job.</summary>
public sealed record GetJobTimelineQuery(Guid TenantId, Guid JobId) : IRequest<Result<PaginatedResult<TimelineStepDto>>>;

/// <summary>Retrieves the scheduling constraints evaluated for a job.</summary>
public sealed record GetJobConstraintsQuery(Guid TenantId, Guid JobId) : IRequest<Result<PaginatedResult<ConstraintDto>>>;

/// <summary>Retrieves fresh predictive scheduling insights for a job (never cached).</summary>
public sealed record GetJobPredictiveInsightsQuery(Guid TenantId, Guid JobId) : IRequest<Result<PredictiveInsightsDto>>;

/// <summary>Retrieves the calendar week grid starting at <paramref name="WeekStart"/>, merged from WorkOrders + PMScheduling.</summary>
public sealed record GetSchedulingCalendarWeekQuery(Guid TenantId, DateOnly WeekStart) : IRequest<Result<CalendarWeekDto>>;

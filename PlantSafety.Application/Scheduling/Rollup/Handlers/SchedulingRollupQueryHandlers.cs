// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.Rollup
//  Layer:          Application
//  File:           SchedulingRollupQueryHandlers.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handlers for every Scheduling.Rollup query. Every handler is a thin composition
//                  layer that fans out via ISender to the real Operational.WorkOrders and
//                  Scheduling.PMScheduling query surfaces (and, for predictive insights, directly to
//                  Operational.PredictiveMaintenance's alert repository/calculator, mirroring
//                  Operational.WorkOrders' own reuse-point handler). No new domain/aggregate
//                  concept is introduced here -- see docs/Phase5/ui-gaps-scoping.md "Scheduling"
//                  section. A "Job" resolves to a WorkOrder first; if not found there, to a
//                  PMScheduling record. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;
using PlantSafety.Application.Operational.WorkOrders.Models;
using PlantSafety.Application.Operational.WorkOrders.Queries;
using PlantSafety.Application.Scheduling.PMScheduling.Models;
using PlantSafety.Application.Scheduling.PMScheduling.Queries;
using PlantSafety.Application.Scheduling.Rollup.Models;
using PlantSafety.Application.Scheduling.Rollup.Queries;

namespace PlantSafety.Application.Scheduling.Rollup.Handlers;

/// <summary>Handles <see cref="GetJobSummaryQuery"/> by rolling up WorkOrder + PMScheduling status counts.</summary>
public sealed class GetJobSummaryQueryHandler(ISender sender, ILogger<GetJobSummaryQueryHandler> logger)
    : IRequestHandler<GetJobSummaryQuery, Result<JobSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<JobSummaryDto>> Handle(GetJobSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pending = 0;
            var inProgress = 0;
            var completed = 0;

            var workOrderSummary = await sender.Send(new GetWorkOrderSummaryQuery(request.TenantId), cancellationToken);
            if (workOrderSummary.IsSuccess)
            {
                pending += workOrderSummary.Value.OpenCount;
                inProgress += workOrderSummary.Value.InProgressCount;
                completed += workOrderSummary.Value.CompletedCount;
            }

            var now = DateTime.UtcNow;
            var pmResult = await sender.Send(
                new GetPMSchedulingQuery(request.TenantId, SiteId: null, From: now.AddDays(-365), To: now.AddDays(90)),
                cancellationToken);
            if (pmResult.IsSuccess)
            {
                foreach (var item in pmResult.Value.Items)
                {
                    switch (item.Status)
                    {
                        case "Created": pending++; break;
                        case "Active": inProgress++; break;
                        case "Completed": completed++; break;
                        // Closed/Archived PMScheduling records are excluded from the dashboard
                        // counts, consistent with WorkOrder's exclusion of Cancelled records.
                    }
                }
            }

            return Result<JobSummaryDto>.Success(new JobSummaryDto
            {
                PendingCount = pending,
                InProgressCount = inProgress,
                CompletedCount = completed,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute Job summary for Tenant {TenantId}", request.TenantId);
            return Result<JobSummaryDto>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobSummaryFailed", "Unable to retrieve the job summary at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetResourceSummaryQuery"/> by bucketing WorkOrders' technician workload read model.</summary>
public sealed class GetResourceSummaryQueryHandler(ISender sender, ILogger<GetResourceSummaryQueryHandler> logger)
    : IRequestHandler<GetResourceSummaryQuery, Result<ResourceSummaryDto>>
{
    /// <inheritdoc/>
    public async Task<Result<ResourceSummaryDto>> Handle(GetResourceSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var workloadResult = await sender.Send(new GetTechnicianWorkloadQuery(request.TenantId, 1, 500), cancellationToken);
            if (workloadResult.IsFailure)
            {
                return Result<ResourceSummaryDto>.Failure(workloadResult.Error);
            }

            var active = 0;
            var overallocated = 0;
            foreach (var technician in workloadResult.Value.Items)
            {
                if (string.Equals(technician.AvailabilityStatus, "Overloaded", StringComparison.OrdinalIgnoreCase))
                {
                    overallocated++;
                }
                else
                {
                    active++;
                }
            }

            return Result<ResourceSummaryDto>.Success(new ResourceSummaryDto
            {
                ActiveCount = active,
                OverallocatedCount = overallocated,
                IdleCount = 0,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute Resource summary for Tenant {TenantId}", request.TenantId);
            return Result<ResourceSummaryDto>.Failure(new ApplicationError(
                "Scheduling.Rollup.ResourceSummaryFailed", "Unable to retrieve the resource summary at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="ListUpcomingJobsQuery"/> by merging WorkOrder calendar events + PMScheduling items into a single "Job" list.</summary>
public sealed class ListUpcomingJobsQueryHandler(ISender sender, ILogger<ListUpcomingJobsQueryHandler> logger)
    : IRequestHandler<ListUpcomingJobsQuery, Result<PaginatedResult<UpcomingJobDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<UpcomingJobDto>>> Handle(ListUpcomingJobsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber.ClampPageNumber();
            var pageSize = request.PageSize.ClampPageSize();
            var now = DateTime.UtcNow;

            var jobs = new List<UpcomingJobDto>();

            var calendarResult = await sender.Send(
                new GetMaintenanceCalendarQuery(request.TenantId, DateOnly.FromDateTime(now), DateOnly.FromDateTime(now.AddDays(90))),
                cancellationToken);
            if (calendarResult.IsSuccess)
            {
                foreach (var evt in calendarResult.Value.Items)
                {
                    jobs.Add(new UpcomingJobDto
                    {
                        JobId = evt.EventId,
                        JobCode = evt.WorkOrderCode ?? "WO",
                        ResourceName = string.IsNullOrWhiteSpace(evt.TechnicianName) ? evt.EquipmentName : evt.TechnicianName,
                        StartUtc = evt.StartUtc,
                        EndUtc = evt.EndUtc,
                        Status = NormalizeWorkOrderStatus(evt.Status),
                    });
                }
            }

            var pmResult = await sender.Send(
                new GetPMSchedulingQuery(request.TenantId, SiteId: null, From: now.AddDays(-1), To: now.AddDays(90)),
                cancellationToken);
            if (pmResult.IsSuccess)
            {
                foreach (var item in pmResult.Value.Items)
                {
                    var normalizedStatus = NormalizePmSchedulingStatus(item.Status);
                    if (normalizedStatus is null)
                    {
                        continue; // Closed/Archived PMScheduling records are not "upcoming".
                    }

                    var start = item.DueAtUtc ?? item.CreatedAtUtc;
                    jobs.Add(new UpcomingJobDto
                    {
                        JobId = item.PMSchedulingId,
                        JobCode = SyntheticPmJobCode(item.PMSchedulingId),
                        ResourceName = item.EquipmentDescription,
                        StartUtc = new DateTimeOffset(DateTime.SpecifyKind(start, DateTimeKind.Utc)),
                        EndUtc = new DateTimeOffset(DateTime.SpecifyKind(start.AddHours(4), DateTimeKind.Utc)),
                        Status = normalizedStatus,
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(request.StatusFilter))
            {
                jobs = jobs.Where(j => string.Equals(j.Status, request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            jobs = jobs.OrderBy(j => j.StartUtc).ToList();

            var total = jobs.Count;
            var page = jobs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResult<UpcomingJobDto>>.Success(new PaginatedResult<UpcomingJobDto>(page, total, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list upcoming jobs for Tenant {TenantId}", request.TenantId);
            return Result<PaginatedResult<UpcomingJobDto>>.Failure(new ApplicationError(
                "Scheduling.Rollup.UpcomingJobsListFailed", "Unable to retrieve upcoming jobs at this time.", Guid.NewGuid()));
        }
    }

    internal static string NormalizeWorkOrderStatus(string status) => status switch
    {
        "Open" or "Assigned" => "Pending",
        "InProgress" or "OnHold" => "InProgress",
        "Completed" => "Completed",
        "Cancelled" => "Cancelled",
        _ => status,
    };

    internal static string? NormalizePmSchedulingStatus(string status) => status switch
    {
        "Created" => "Pending",
        "Active" => "InProgress",
        "Completed" => "Completed",
        _ => null, // Closed/Archived
    };

    internal static string SyntheticPmJobCode(Guid pmSchedulingId) => $"PM-{pmSchedulingId.ToString("N")[..6].ToUpperInvariant()}";
}

/// <summary>
/// Handles <see cref="GetJobDetailQuery"/>, <see cref="GetJobTimelineQuery"/>, <see cref="GetJobConstraintsQuery"/>,
/// and <see cref="GetJobPredictiveInsightsQuery"/> by resolving the identifier against WorkOrder first, then
/// PMScheduling. Grouped into a single file/class family since all four share the identical resolve-then-branch
/// shape over the same two source modules.
/// </summary>
public sealed class GetJobDetailQueryHandler(ISender sender, ILogger<GetJobDetailQueryHandler> logger)
    : IRequestHandler<GetJobDetailQuery, Result<JobDetailDto>>
{
    /// <inheritdoc/>
    public async Task<Result<JobDetailDto>> Handle(GetJobDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var workOrderResult = await sender.Send(new GetWorkOrderByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (workOrderResult.IsSuccess)
            {
                var wo = workOrderResult.Value;

                var resourceId = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(wo.TechnicianName))
                {
                    var workloadResult = await sender.Send(new GetTechnicianWorkloadQuery(request.TenantId, 1, 500), cancellationToken);
                    if (workloadResult.IsSuccess)
                    {
                        var match = workloadResult.Value.Items.FirstOrDefault(t =>
                            string.Equals(t.TechnicianName, wo.TechnicianName, StringComparison.OrdinalIgnoreCase));
                        if (match is not null)
                        {
                            resourceId = match.TechnicianId;
                        }
                    }
                }

                var durationMinutes = 240;
                var tasksResult = await sender.Send(new GetWorkOrderTasksQuery(request.TenantId, request.JobId, 1, 200), cancellationToken);
                if (tasksResult.IsSuccess && tasksResult.Value.Items.Count > 0)
                {
                    durationMinutes = tasksResult.Value.Items.Sum(t => t.DurationMinutes);
                }

                return Result<JobDetailDto>.Success(new JobDetailDto
                {
                    JobId = wo.WorkOrderId,
                    JobCode = wo.WorkOrderCode,
                    Status = wo.Status,
                    ResourceName = string.IsNullOrWhiteSpace(wo.TechnicianName) ? "Unassigned" : wo.TechnicianName,
                    ResourceId = resourceId,
                    DurationMinutes = durationMinutes,
                    CreatedAtUtc = wo.CreatedAtUtc,
                    UpdatedAtUtc = wo.UpdatedAtUtc,
                    OperatorName = wo.TechnicianName,
                });
            }

            var pmResult = await sender.Send(new GetPMSchedulingByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (pmResult.IsSuccess)
            {
                var pm = pmResult.Value;
                var pmCreatedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(pm.CreatedAtUtc, DateTimeKind.Utc));
                var pmUpdatedAtUtc = pm.UpdatedAtUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(pm.UpdatedAtUtc.Value, DateTimeKind.Utc))
                    : pmCreatedAtUtc;

                return Result<JobDetailDto>.Success(new JobDetailDto
                {
                    JobId = pm.PMSchedulingId,
                    JobCode = ListUpcomingJobsQueryHandler.SyntheticPmJobCode(pm.PMSchedulingId),
                    Status = pm.Status,
                    ResourceName = pm.EquipmentDescription,
                    ResourceId = pm.EquipmentId,
                    DurationMinutes = 240,
                    CreatedAtUtc = pmCreatedAtUtc,
                    UpdatedAtUtc = pmUpdatedAtUtc,
                    OperatorName = string.Empty,
                });
            }

            return Result<JobDetailDto>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobNotFound", "The requested job could not be found.", request.JobId, ErrorSeverity.NotFound));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve Job {JobId}", request.JobId);
            return Result<JobDetailDto>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobDetailRetrievalFailed", "Unable to retrieve job detail at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetJobTimelineQuery"/>.</summary>
public sealed class GetJobTimelineQueryHandler(ISender sender, ILogger<GetJobTimelineQueryHandler> logger)
    : IRequestHandler<GetJobTimelineQuery, Result<PaginatedResult<TimelineStepDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<TimelineStepDto>>> Handle(GetJobTimelineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var workOrderResult = await sender.Send(new GetWorkOrderByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (workOrderResult.IsSuccess)
            {
                var tasksResult = await sender.Send(new GetWorkOrderTasksQuery(request.TenantId, request.JobId, 1, 200), cancellationToken);
                if (tasksResult.IsFailure)
                {
                    return Result<PaginatedResult<TimelineStepDto>>.Failure(tasksResult.Error);
                }

                var cursor = workOrderResult.Value.CreatedAtUtc;
                var steps = new List<TimelineStepDto>();
                foreach (var task in tasksResult.Value.Items.OrderBy(t => t.StepNumber))
                {
                    var start = cursor;
                    var end = cursor.AddMinutes(Math.Max(1, task.DurationMinutes));
                    steps.Add(new TimelineStepDto
                    {
                        StepNumber = task.StepNumber,
                        Description = task.Description,
                        StartUtc = start,
                        EndUtc = end,
                        Status = task.Status,
                    });
                    cursor = end;
                }

                return Result<PaginatedResult<TimelineStepDto>>.Success(new PaginatedResult<TimelineStepDto>(steps, steps.Count, 1, Math.Max(steps.Count, 1)));
            }

            var pmResult = await sender.Send(new GetPMSchedulingByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (pmResult.IsSuccess)
            {
                var pm = pmResult.Value;
                var start = pm.DueAtUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(pm.DueAtUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset(DateTime.SpecifyKind(pm.CreatedAtUtc, DateTimeKind.Utc));

                // PMScheduling records do not track sub-tasks; a single synthetic step represents
                // the whole scheduled activity (reduced-fidelity, documented in SchedulingRollupModels.cs).
                var steps = new List<TimelineStepDto>
                {
                    new()
                    {
                        StepNumber = 1,
                        Description = pm.PmTaskType,
                        StartUtc = start,
                        EndUtc = start.AddHours(4),
                        Status = pm.Status,
                    },
                };

                return Result<PaginatedResult<TimelineStepDto>>.Success(new PaginatedResult<TimelineStepDto>(steps, 1, 1, 1));
            }

            return Result<PaginatedResult<TimelineStepDto>>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobNotFound", "The requested job could not be found.", request.JobId, ErrorSeverity.NotFound));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve timeline for Job {JobId}", request.JobId);
            return Result<PaginatedResult<TimelineStepDto>>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobTimelineRetrievalFailed", "Unable to retrieve the job timeline at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>Handles <see cref="GetJobConstraintsQuery"/>.</summary>
public sealed class GetJobConstraintsQueryHandler(ISender sender, ILogger<GetJobConstraintsQueryHandler> logger)
    : IRequestHandler<GetJobConstraintsQuery, Result<PaginatedResult<ConstraintDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<ConstraintDto>>> Handle(GetJobConstraintsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var workOrderResult = await sender.Send(new GetWorkOrderByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (workOrderResult.IsSuccess)
            {
                var wo = workOrderResult.Value;
                var now = DateTimeOffset.UtcNow;

                var constraints = new List<ConstraintDto>
                {
                    new()
                    {
                        ConstraintType = "Assignment",
                        Description = "A technician must be assigned before work can start.",
                        IsSatisfied = !string.IsNullOrWhiteSpace(wo.TechnicianName),
                    },
                    new()
                    {
                        ConstraintType = "Schedule",
                        Description = "The job is due on or after today.",
                        IsSatisfied = wo.DueAtUtc >= now || wo.Status == "Completed",
                    },
                    new()
                    {
                        ConstraintType = "Machine",
                        Description = "The target machine is not currently blocked (on hold).",
                        IsSatisfied = wo.Status != "OnHold",
                    },
                    new()
                    {
                        ConstraintType = "Safety",
                        Description = "Critical-priority jobs must have an assigned technician.",
                        IsSatisfied = wo.Priority != "Critical" || !string.IsNullOrWhiteSpace(wo.TechnicianName),
                    },
                };

                return Result<PaginatedResult<ConstraintDto>>.Success(new PaginatedResult<ConstraintDto>(constraints, constraints.Count, 1, constraints.Count));
            }

            var pmResult = await sender.Send(new GetPMSchedulingByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (pmResult.IsSuccess)
            {
                // PMScheduling records carry no constraint data (documented in SchedulingRollupModels.cs).
                return Result<PaginatedResult<ConstraintDto>>.Success(new PaginatedResult<ConstraintDto>([], 0, 1, 1));
            }

            return Result<PaginatedResult<ConstraintDto>>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobNotFound", "The requested job could not be found.", request.JobId, ErrorSeverity.NotFound));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve constraints for Job {JobId}", request.JobId);
            return Result<PaginatedResult<ConstraintDto>>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobConstraintsRetrievalFailed", "Unable to retrieve job constraints at this time.", Guid.NewGuid()));
        }
    }
}

/// <summary>
/// Handles <see cref="GetJobPredictiveInsightsQuery"/>. For WorkOrder-sourced jobs, delegates to
/// Operational.WorkOrders' own predictive-insights reuse query. For PMScheduling-sourced jobs,
/// invokes the same Operational.PredictiveMaintenance alert repository/calculator directly against
/// the record's EquipmentId, mirroring GetWorkOrderPredictiveInsightsQueryHandler's approach.
/// </summary>
public sealed class GetJobPredictiveInsightsQueryHandler(
    ISender sender,
    IPredictiveMaintenanceAlertRepository predictiveAlertRepository,
    ILogger<GetJobPredictiveInsightsQueryHandler> logger)
    : IRequestHandler<GetJobPredictiveInsightsQuery, Result<PredictiveInsightsDto>>
{
    private readonly PredictiveMaintenanceInsightsCalculator _calculator = new();

    /// <inheritdoc/>
    public async Task<Result<PredictiveInsightsDto>> Handle(GetJobPredictiveInsightsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var workOrderResult = await sender.Send(new GetWorkOrderByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (workOrderResult.IsSuccess)
            {
                var wo = workOrderResult.Value;
                var insightsResult = await sender.Send(new GetWorkOrderPredictiveInsightsQuery(request.TenantId, request.JobId), cancellationToken);
                if (insightsResult.IsFailure)
                {
                    return Result<PredictiveInsightsDto>.Failure(insightsResult.Error);
                }

                return Result<PredictiveInsightsDto>.Success(BuildDto(insightsResult.Value.RiskScore, wo.DueAtUtc, insightsResult.Value.ModelVersion));
            }

            var pmResult = await sender.Send(new GetPMSchedulingByIdQuery(request.TenantId, request.JobId), cancellationToken);
            if (pmResult.IsSuccess)
            {
                var pm = pmResult.Value;
                var (alerts, _) = await predictiveAlertRepository.GetPagedAsync(
                    request.TenantId, equipmentId: pm.EquipmentId, activeOnly: false, pageNumber: 1, pageSize: 50, cancellationToken);

                var now = DateTime.UtcNow;
                var latest = alerts.OrderByDescending(a => a.PredictedAtUtc).FirstOrDefault();
                var dueAtUtc = pm.DueAtUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(pm.DueAtUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset(DateTime.SpecifyKind(pm.CreatedAtUtc, DateTimeKind.Utc));

                if (latest is null)
                {
                    return Result<PredictiveInsightsDto>.Success(BuildDto(0.0, dueAtUtc, _calculator.Predictor.ModelVersion));
                }

                var prediction = _calculator.PredictForAlert(latest.Status, latest.PredictedAtUtc, now);
                return Result<PredictiveInsightsDto>.Success(BuildDto(Math.Round(prediction.Output.FailureLikelihood, 3), dueAtUtc, prediction.ModelVersion));
            }

            return Result<PredictiveInsightsDto>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobNotFound", "The requested job could not be found.", request.JobId, ErrorSeverity.NotFound));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve predictive insights for Job {JobId}", request.JobId);
            return Result<PredictiveInsightsDto>.Failure(new ApplicationError(
                "Scheduling.Rollup.JobPredictiveInsightsFailed", "Unable to retrieve predictive scheduling insights at this time.", Guid.NewGuid()));
        }
    }

    /// <summary>
    /// Computes the rollup-layer RecommendedStartUtc/RecommendedEndUtc: the job's due date, pulled
    /// forward 24h when risk exceeds 0.6, plus a nominal 4-hour work window. Not persisted anywhere.
    /// </summary>
    private static PredictiveInsightsDto BuildDto(double riskScore, DateTimeOffset dueAtUtc, string modelVersion)
    {
        var recommendedStart = riskScore > 0.6 ? dueAtUtc.AddHours(-24) : dueAtUtc;
        return new PredictiveInsightsDto
        {
            RiskScore = riskScore,
            RecommendedStartUtc = recommendedStart,
            RecommendedEndUtc = recommendedStart.AddHours(4),
            ModelVersion = modelVersion,
        };
    }
}

/// <summary>Handles <see cref="GetSchedulingCalendarWeekQuery"/> by merging WorkOrder calendar events + PMScheduling items into a single 7-day grid.</summary>
public sealed class GetSchedulingCalendarWeekQueryHandler(ISender sender, ILogger<GetSchedulingCalendarWeekQueryHandler> logger)
    : IRequestHandler<GetSchedulingCalendarWeekQuery, Result<CalendarWeekDto>>
{
    /// <inheritdoc/>
    public async Task<Result<CalendarWeekDto>> Handle(GetSchedulingCalendarWeekQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var weekStart = request.WeekStart;
            var weekEnd = weekStart.AddDays(6);

            var jobs = new List<CalendarJobDto>();

            var calendarResult = await sender.Send(new GetMaintenanceCalendarQuery(request.TenantId, weekStart, weekEnd), cancellationToken);
            if (calendarResult.IsSuccess)
            {
                foreach (var evt in calendarResult.Value.Items)
                {
                    jobs.Add(new CalendarJobDto
                    {
                        JobId = evt.EventId,
                        JobCode = evt.WorkOrderCode ?? "WO",
                        ResourceName = string.IsNullOrWhiteSpace(evt.TechnicianName) ? evt.EquipmentName : evt.TechnicianName,
                        StartUtc = evt.StartUtc,
                        EndUtc = evt.EndUtc,
                        Status = ListUpcomingJobsQueryHandler.NormalizeWorkOrderStatus(evt.Status),
                    });
                }
            }

            var weekStartUtc = weekStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var weekEndUtc = weekEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            var pmResult = await sender.Send(new GetPMSchedulingQuery(request.TenantId, SiteId: null, From: weekStartUtc, To: weekEndUtc), cancellationToken);
            if (pmResult.IsSuccess)
            {
                foreach (var item in pmResult.Value.Items)
                {
                    if (item.DueAtUtc is null)
                    {
                        continue;
                    }

                    var start = new DateTimeOffset(DateTime.SpecifyKind(item.DueAtUtc.Value, DateTimeKind.Utc));
                    jobs.Add(new CalendarJobDto
                    {
                        JobId = item.PMSchedulingId,
                        JobCode = ListUpcomingJobsQueryHandler.SyntheticPmJobCode(item.PMSchedulingId),
                        ResourceName = item.EquipmentDescription,
                        StartUtc = start,
                        EndUtc = start.AddHours(4),
                        Status = ListUpcomingJobsQueryHandler.NormalizePmSchedulingStatus(item.Status) ?? item.Status,
                    });
                }
            }

            var days = new List<CalendarDayDto>();
            for (var i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                var dayJobs = jobs.Where(j => DateOnly.FromDateTime(j.StartUtc.UtcDateTime) == date)
                    .OrderBy(j => j.StartUtc)
                    .ToList();
                days.Add(new CalendarDayDto { Date = date, Jobs = dayJobs });
            }

            return Result<CalendarWeekDto>.Success(new CalendarWeekDto { WeekStart = weekStart, Days = days });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve scheduling calendar week for Tenant {TenantId}", request.TenantId);
            return Result<CalendarWeekDto>.Failure(new ApplicationError(
                "Scheduling.Rollup.CalendarWeekRetrievalFailed", "Unable to retrieve the scheduling calendar at this time.", Guid.NewGuid()));
        }
    }
}

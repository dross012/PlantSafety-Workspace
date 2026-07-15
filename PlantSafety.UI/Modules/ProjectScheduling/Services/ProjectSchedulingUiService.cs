// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer orchestration + mapping service for the Project
//   Scheduling module. Calls the shared PlanningProjectsApiClient (the
//   Project Scheduling module is one of three UI surfaces over the single
//   PlanningProjects backend bounded context — see
//   Modules/PlanningProjects/ApiClients/PlanningProjectsApiClient.cs header)
//   and projects the results into Gantt timeline bars and key-date schedule
//   events for ProjectTimeline.razor / ScheduleCalendar.razor. Named/shaped
//   after the platform's standard Mapster DTO -&gt; ViewModel convention; see
//   PlanningProjectsMappingService header for the package-absence rationale.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.PlanningProjects.ApiClients;
using PlantSafety.UI.Modules.PlanningProjects.Mappings;
using PlantSafety.UI.Modules.PlanningProjects.Models;

namespace PlantSafety.UI.Modules.ProjectScheduling.Services;

/// <summary>Orchestrates loading and mapping of schedule data for the Project Scheduling dashboard and detail pages.</summary>
public sealed class ProjectSchedulingUiService
{
    private readonly PlanningProjectsApiClient _apiClient;

    /// <summary>Initializes a new instance of <see cref="ProjectSchedulingUiService"/>.</summary>
    /// <param name="apiClient">The shared PlanningProjects typed API client.</param>
    public ProjectSchedulingUiService(PlanningProjectsApiClient apiClient) => _apiClient = apiClient;

    /// <summary>
    /// Loads the tenant's planning project list and projects it into Gantt timeline bars and
    /// target-completion schedule events for the Project Scheduling dashboard.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="fromUtc">Inclusive start of the target-completion date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the target-completion date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task<(bool IsSuccess, IReadOnlyList<ProjectTimelineItemViewModel> TimelineItems, IReadOnlyList<ScheduleEventViewModel> Events, string? Error)>
        LoadScheduleOverviewAsync(Guid tenantId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var envelope = await _apiClient.GetProjectsAsync(tenantId, siteId: null, fromUtc, toUtc, cancellationToken);

        if (!envelope.IsSuccess)
        {
            var error = envelope.IsProblem
                ? envelope.Problem!.Detail
                : $"Failed to load planning projects: {envelope.FaultType}";
            return (false, [], [], error);
        }

        var items = envelope.Data!.Items;
        var snapshotUtc = envelope.Data!.GeneratedAt;
        var nowUtc = DateTime.UtcNow;

        var timelineItems = items.Select(i => i.ToTimelineItemViewModel(snapshotUtc)).ToList();

        var events = items.Select(i => new ScheduleEventViewModel
        {
            Label = i.Name,
            DateUtc = i.TargetCompletionUtc,
            Kind = ScheduleEventKind.PlannedEnd,
            IsOverdue = i.TargetCompletionUtc < nowUtc && i.Status.Trim().ToLowerInvariant() is not ("completed" or "cancelled" or "archived"),
        }).ToList();

        return (true, timelineItems, events, null);
    }

    /// <summary>Loads a single project's full detail for the scheduling detail page.</summary>
    /// <param name="projectId">The project to load.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<SafetyProjectDto>> LoadProjectDetailAsync(Guid projectId, CancellationToken cancellationToken = default)
        => _apiClient.GetProjectByIdAsync(projectId, cancellationToken);

    /// <summary>Projects a full Safety Project detail DTO into its planned/actual key-date schedule events.</summary>
    /// <param name="dto">The project detail DTO.</param>
    public static IReadOnlyList<ScheduleEventViewModel> ToScheduleEvents(SafetyProjectDto dto)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var isTerminal = dto.Status.Trim().ToLowerInvariant() is "completed" or "cancelled" or "archived";

        var events = new List<ScheduleEventViewModel>
        {
            new()
            {
                Label = dto.Name,
                DateUtc = dto.PlannedStartUtc,
                Kind = ScheduleEventKind.PlannedStart,
                IsOverdue = dto.PlannedStartUtc < nowUtc && dto.ActualStartUtc is null && !isTerminal,
            },
            new()
            {
                Label = dto.Name,
                DateUtc = dto.PlannedEndUtc,
                Kind = ScheduleEventKind.PlannedEnd,
                IsOverdue = dto.PlannedEndUtc < nowUtc && dto.ActualEndUtc is null && !isTerminal,
            },
        };

        if (dto.ActualStartUtc is { } actualStart)
        {
            events.Add(new ScheduleEventViewModel { Label = dto.Name, DateUtc = actualStart, Kind = ScheduleEventKind.ActualStart });
        }

        if (dto.ActualEndUtc is { } actualEnd)
        {
            events.Add(new ScheduleEventViewModel { Label = dto.Name, DateUtc = actualEnd, Kind = ScheduleEventKind.ActualEnd });
        }

        return events;
    }
}

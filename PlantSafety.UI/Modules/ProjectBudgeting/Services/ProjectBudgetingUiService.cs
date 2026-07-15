// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer orchestration + mapping service for the Project
//   Budgeting module. Calls the shared PlanningProjectsApiClient (the
//   Project Budgeting module is one of three UI surfaces over the single
//   PlanningProjects backend bounded context — see
//   Modules/PlanningProjects/ApiClients/PlanningProjectsApiClient.cs header),
//   then enriches each list row with its planned-budget figure via a
//   best-effort parallel detail fetch (the list endpoint does not itself
//   return budget data). Named/shaped after the platform's standard Mapster
//   DTO -&gt; ViewModel convention; implemented as hand-written methods
//   because PlantSafety.UI.csproj does not reference Mapster (see
//   PlanningProjectsMappingService header for the full rationale).
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
using PlantSafety.UI.Modules.PlanningProjects.Models;
using PlantSafety.UI.Modules.ProjectBudgeting.Mappings;
using PlantSafety.UI.Modules.ProjectBudgeting.Models;

namespace PlantSafety.UI.Modules.ProjectBudgeting.Services;

/// <summary>Orchestrates loading and mapping of budget data for the Project Budgeting dashboard and detail pages.</summary>
public sealed class ProjectBudgetingUiService
{
    private readonly PlanningProjectsApiClient _apiClient;

    /// <summary>Initializes a new instance of <see cref="ProjectBudgetingUiService"/>.</summary>
    /// <param name="apiClient">The shared PlanningProjects typed API client.</param>
    public ProjectBudgetingUiService(PlanningProjectsApiClient apiClient) => _apiClient = apiClient;

    /// <summary>
    /// Loads the tenant's planning project list and enriches each row with its planned-budget
    /// figure via a best-effort parallel detail fetch. Rows whose detail fetch fails degrade
    /// gracefully to <c>IsBudgetKnown = false</c> rather than failing the whole dashboard.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="fromUtc">Inclusive start of the target-completion date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the target-completion date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>
    /// A success flag plus either the mapped budget rows or a sanitized error message,
    /// mirroring the ApiResponseEnvelope discriminated shape for the list call itself
    /// (enrichment failures are per-row, not fatal).
    /// </returns>
    public async Task<(bool IsSuccess, IReadOnlyList<ProjectBudgetViewModel> Rows, string? Error)> LoadBudgetOverviewAsync(
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var listEnvelope = await _apiClient.GetProjectsAsync(tenantId, siteId: null, fromUtc, toUtc, cancellationToken);

        if (!listEnvelope.IsSuccess)
        {
            var error = listEnvelope.IsProblem
                ? listEnvelope.Problem!.Detail
                : $"Failed to load planning projects: {listEnvelope.FaultType}";
            return (false, [], error);
        }

        var items = listEnvelope.Data!.Items;

        var detailTasks = items
            .Select(item => _apiClient.GetProjectByIdAsync(item.ProjectId, cancellationToken))
            .ToArray();

        var detailEnvelopes = await Task.WhenAll(detailTasks);

        var rows = items.Zip(detailEnvelopes, (item, detail) => detail.IsSuccess
                ? item.ToBudgetViewModel(detail.Data!.PlannedBudget, isBudgetKnown: true)
                : item.ToBudgetViewModel(plannedBudget: null, isBudgetKnown: false))
            .OrderByDescending(r => r.PlannedBudget ?? 0m)
            .ToList();

        return (true, rows, null);
    }

    /// <summary>Computes the sum of all known planned-budget figures across the given rows.</summary>
    /// <param name="rows">The budget rows to aggregate.</param>
    public static decimal ComputeTotalPlannedBudget(IReadOnlyList<ProjectBudgetViewModel> rows) =>
        rows.Where(r => r.IsBudgetKnown).Sum(r => r.PlannedBudget ?? 0m);

    /// <summary>Loads a single project's full detail for the budgeting detail page.</summary>
    /// <param name="projectId">The project to load.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<SafetyProjectDto>> LoadProjectDetailAsync(Guid projectId, CancellationToken cancellationToken = default)
        => _apiClient.GetProjectByIdAsync(projectId, cancellationToken);
}

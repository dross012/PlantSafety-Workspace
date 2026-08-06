// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Planning Projects module. Wraps
//   all HTTP communication for the PlanningProjects read-model list endpoint
//   and the Features/PlanningProjects/Projects detail endpoint. All methods
//   return ApiResponseEnvelope<T>. No HttpClient calls are permitted outside
//   this class for the Planning Projects, Project Budgeting, or Project
//   Scheduling modules (all three share this one client — they are three
//   distinct UI surfaces over the same PlanningProjects backend bounded
//   context, mirroring RouteDefinitions' single "Planning Projects" section).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.PlanningProjects.Models;

namespace PlantSafety.UI.Modules.PlanningProjects.ApiClients;

/// <summary>
/// Typed API client for the Planning Projects / Project Budgeting / Project
/// Scheduling modules. Components MUST inject this class and call its methods
/// rather than constructing HTTP requests directly.
/// </summary>
public sealed class PlanningProjectsApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="PlanningProjectsApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public PlanningProjectsApiClient(HttpClient http, ILogger<PlanningProjectsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves the planning-projects read-model list for the given tenant within
    /// an inclusive UTC date range (matched against each project's target
    /// completion date). Backed by GET <c>v1/planning-projects</c>.
    /// </summary>
    /// <param name="tenantId">Tenant identifier resolved from the active session.</param>
    /// <param name="siteId">Optional site filter. Null returns data for all accessible sites.</param>
    /// <param name="fromUtc">Inclusive start of the date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the planning projects list or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<PlanningProjectsListResponseDto>> GetProjectsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}" +
                     $"&From={Uri.EscapeDataString(fromUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                     $"&To={Uri.EscapeDataString(toUtc.ToString("O", CultureInfo.InvariantCulture))}";

        if (siteId.HasValue)
        {
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        }

        return GetAsync<PlanningProjectsListResponseDto>($"v1/planning-projects?{query}", cancellationToken);
    }

    /// <summary>
    /// Retrieves the full Safety Project detail for a single project. Backed by
    /// GET <c>v1/features/planning-projects/projects/{projectId}</c>.
    /// </summary>
    /// <param name="projectId">The unique project identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the full <see cref="SafetyProjectDto"/> or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<SafetyProjectDto>> GetProjectByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
        => GetAsync<SafetyProjectDto>($"v1/features/planning-projects/projects/{projectId}", cancellationToken);

    /// <summary>
    /// Creates a new Project (Planning status). The project's <c>TargetCompletionUtc</c> IS the
    /// scheduling input — Project Scheduling and Project Budgeting are read-only projections over
    /// this same data, so no separate scheduling-create endpoint exists. Backed by
    /// POST <c>v1/planning-projects/projects</c> (requires the Project Write authorization policy).
    /// </summary>
    public Task<ApiResponseEnvelope<ProjectStatusResult>> CreateProjectAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CreateProjectRequest, ProjectStatusResult>(
            "v1/planning-projects/projects", request, cancellationToken);

    /// <summary>
    /// Adds a milestone to an existing Project — the "Milestones" scheduling checkpoint input.
    /// Backed by POST <c>v1/planning-projects/projects/{projectId}/milestones</c> (requires the
    /// Project Write authorization policy).
    /// </summary>
    public Task<ApiResponseEnvelope<ProjectStatusResult>> AddMilestoneAsync(
        Guid projectId,
        AddProjectMilestoneRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<AddProjectMilestoneRequest, ProjectStatusResult>(
            $"v1/planning-projects/projects/{projectId}/milestones", request, cancellationToken);
}

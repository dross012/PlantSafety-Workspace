// ============================================================================
// File: ProjectsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Application-layer
//              Features/PlanningProjects/Projects Commands and Queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/planning-projects/projects/...
// - Envelope fields are populated via the shared RequestEnvelopeFactory.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.PlanningProjects.Projects.Contracts.Requests;
using PlantSafety.Application.Features.PlanningProjects.Commands.Projects.CreateSafetyProject;
using PlantSafety.Application.Features.PlanningProjects.Models.Projects;
using PlantSafety.Application.Features.PlanningProjects.Queries.Projects.GetSafetyProject;

namespace PlantSafety.Api.Features.PlanningProjects.Projects.Controllers.V1;

/// <summary>V1 controller for Safety Project commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/planning-projects/projects")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Projects")]
public sealed class ProjectsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ProjectsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ProjectsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Safety Project in Draft status.</summary>
    /// <param name="request">Project fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new ProjectId.</returns>
    /// <response code="200">Project created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateSafetyProjectRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateSafetyProjectCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            Name = request.Name,
            Description = request.Description,
            Priority = request.Priority,
            ProjectOwnerId = request.ProjectOwnerId,
            ProjectManagerId = request.ProjectManagerId,
            PlannedStartUtc = request.PlannedStartUtc,
            PlannedEndUtc = request.PlannedEndUtc,
            PlannedBudget = request.PlannedBudget
        };

        var projectId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(projectId, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single Safety Project by identifier.</summary>
    /// <param name="projectId">Project to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="SafetyProjectDto"/>.</returns>
    /// <response code="200">Project found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Project not found.</response>
    [HttpGet("{projectId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<SafetyProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyProjectDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSafetyProjectByIdQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            ProjectId = projectId
        };

        var project = await _sender.Send(query, cancellationToken);

        if (project is null)
            return NotFound(ApiResponse<SafetyProjectDto>.Fail("Project not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<SafetyProjectDto>.Ok(project, env.CorrelationId.ToString()));
    }
}

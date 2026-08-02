// ============================================================================
// File: ProjectV1Controller.cs
// Project: PlantSafety.Api
// Module: PlanningProjects.ProjectAggregate
// Created: 2026-08-02
// Description: V1 controller for the PlanningProjects.ProjectAggregate module's Phase 4 full
//              build-out Create/lifecycle/milestone/by-id command and query endpoints. Distinct
//              from the pre-existing PlanningProjectsV1Controller (read-only list-query vertical,
//              left untouched). Routes all reads/writes through MediatR ISender to the Application
//              layer. Mirrors
//              PlantSafety.Api.SafetyEvents.RootCauseAnalysis.Controllers.V1
//              .RootCauseAnalysisV1Controller.
// ============================================================================

using PlantSafety.Api.PlanningProjects.Authorization;
using PlantSafety.Api.PlanningProjects.Contracts.Requests;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.PlanningProjects.Controllers.V1;

/// <summary>V1 controller for the PlanningProjects.ProjectAggregate module's write and by-id read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/planning-projects/projects")]
[Authorize(Policy = ProjectAuthorizationPolicies.Read)]
[Tags("Project")]
public sealed class ProjectV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ProjectV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ProjectV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Project.</summary>
    [HttpPost]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateProjectCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            Name: request.Name,
            TargetCompletionUtc: request.TargetCompletionUtc,
            OwnerUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a Project (Planning -&gt; Active).</summary>
    [HttpPost("{projectId:guid}/activate")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid projectId,
        [FromBody] ActivateProjectRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateProjectCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            ActivatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Puts a Project on hold (Active -&gt; OnHold).</summary>
    [HttpPost("{projectId:guid}/put-on-hold")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PutOnHoldAsync(
        Guid projectId,
        [FromBody] PutProjectOnHoldRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PutProjectOnHoldCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            PutOnHoldByUserId: env.ActorId,
            Reason: request?.Reason)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resumes a Project from hold (OnHold -&gt; Active).</summary>
    [HttpPost("{projectId:guid}/resume")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResumeAsync(
        Guid projectId,
        [FromBody] ResumeProjectRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResumeProjectCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            ResumedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes a Project (Active -&gt; Completed).</summary>
    [HttpPost("{projectId:guid}/complete")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid projectId,
        [FromBody] CompleteProjectRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteProjectCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            CompletedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Cancels a Project (Planning/Active/OnHold -&gt; Cancelled).</summary>
    [HttpPost("{projectId:guid}/cancel")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelAsync(
        Guid projectId,
        [FromBody] CancelProjectRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CancelProjectCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            CancelledByUserId: env.ActorId,
            Reason: request?.Reason)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a Project (Completed/Cancelled -&gt; Archived).</summary>
    [HttpPost("{projectId:guid}/archive")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid projectId,
        [FromBody] ArchiveProjectRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveProjectCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adds a milestone to a Project.</summary>
    [HttpPost("{projectId:guid}/milestones")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddMilestoneAsync(
        Guid projectId,
        [FromBody] AddProjectMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddProjectMilestoneCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            AddedByUserId: env.ActorId,
            Name: request.Name,
            TargetDateUtc: request.TargetDateUtc)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Marks a Project milestone as achieved.</summary>
    [HttpPost("{projectId:guid}/milestones/{milestoneId:guid}/achieve")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProjectStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AchieveMilestoneAsync(
        Guid projectId,
        Guid milestoneId,
        [FromBody] AchieveProjectMilestoneRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AchieveProjectMilestoneCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            ProjectId: projectId,
            MilestoneId: milestoneId,
            AchievedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Project read-model record by identity, including its current ETag.</summary>
    [HttpGet("{projectId:guid}")]
    [Authorize(Policy = ProjectAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.PlanningProjects.ProjectAggregate.Models.ProjectDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetProjectByIdQuery(env.TenantId, projectId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}

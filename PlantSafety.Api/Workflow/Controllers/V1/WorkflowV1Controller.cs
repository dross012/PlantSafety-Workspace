// ============================================================================
// File: WorkflowV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: V1 controller exposing the Application-layer Workflow module's lifecycle-transition
//              commands (Start/Advance/Approve/Reject/Escalate/Retry/Cancel), definition-authoring
//              commands (CreateDefinition/AddStep/AddTransition/ActivateDefinition), human+machine
//              collaboration commands (MakeAutomatedDecision/SurfaceRecommendation), and read-model
//              queries (GetWorkflowInstance/GetWorkflowDefinitionById/GetPendingApprovalsForActor/
//              GetWorkflowHistory) over HTTP (Phase 5 §5 — human + machine workflow runtime).
//              Mirrors PlantSafety.Api.Analytics.Dashboard.Controllers.V1.DashboardV1Controller's
//              command-dispatch pattern (POST {resource}/{id}/{action}).
//              2026-08-03 (API Contract Validation Agent, platform-wide regression audit): added the
//              definition-authoring and human+machine collaboration actions that were present in
//              PlantSafety.Application.Workflow.Commands but had no HTTP surface -- a CQRS-to-endpoint
//              gap caught by the audit. No UI consumer wired to these yet (WorkflowApiClient covers
//              only the lifecycle-transition set); routes follow the same envelope/error/authorization
//              conventions as the existing actions in this file.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/workflow/... exactly as consumed by
//   PlantSafety.UI.Modules.Workflow.ApiClients.WorkflowApiClient.
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Workflow.Authorization;
using PlantSafety.Api.Workflow.Contracts.Requests;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Workflow.Commands;
using PlantSafety.Application.Workflow.Models;
using PlantSafety.Application.Workflow.Queries;

namespace PlantSafety.Api.Workflow.Controllers.V1;

/// <summary>V1 controller for the Workflow module's lifecycle-transition commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Workflow")]
public sealed class WorkflowV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>Initialises a new instance of <see cref="WorkflowV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    /// <param name="currentUserService">Resolves the authenticated caller's own identity/roles (no impersonation via id substitution).</param>
    public WorkflowV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory, ICurrentUserService currentUserService)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
        _currentUserService = currentUserService;
    }

    /// <summary>Retrieves all WorkflowInstances currently pending an approval decision from the calling actor.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <remarks>
    /// The actor identity/roles are resolved server-side from the authenticated caller
    /// (<see cref="ICurrentUserService"/>), never from client-supplied query parameters -- otherwise
    /// any authenticated caller could pass another user's id/roles and read that user's pending
    /// approval queue (id-substitution / horizontal privilege escalation).
    /// </remarks>
    [HttpGet("approvals/pending")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkflowInstanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkflowInstanceDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingApprovalsAsync(
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPendingApprovalsForActorQuery(env.TenantId, _currentUserService.UserId, _currentUserService.Roles);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single WorkflowInstance read-model record by identity.</summary>
    /// <param name="workflowInstanceId">Identity of the WorkflowInstance record to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("instances/{workflowInstanceId:guid}")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInstanceAsync(
        Guid workflowInstanceId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWorkflowInstanceQuery(env.TenantId, workflowInstanceId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the full step execution history of a WorkflowInstance, in activation order.</summary>
    /// <param name="workflowInstanceId">Identity of the WorkflowInstance whose history is being retrieved.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("instances/{workflowInstanceId:guid}/history")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkflowStepHistoryEntryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistoryAsync(
        Guid workflowInstanceId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWorkflowHistoryQuery(env.TenantId, workflowInstanceId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single WorkflowDefinition and its step graph by identity.</summary>
    /// <param name="workflowDefinitionId">Identity of the WorkflowDefinition record to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("definitions/{workflowDefinitionId:guid}")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDefinitionAsync(
        Guid workflowDefinitionId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWorkflowDefinitionByIdQuery(env.TenantId, workflowDefinitionId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts a new WorkflowInstance from an activated WorkflowDefinition.</summary>
    /// <param name="request">Caller-supplied fields for the new instance.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/start")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAsync(
        [FromBody] StartWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            WorkflowDefinitionId: request.WorkflowDefinitionId,
            InitialContextData: request.InitialContextData,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a human approval decision on a WorkflowInstance's current step.</summary>
    /// <param name="workflowInstanceId">Identity of the instance whose current step is being approved.</param>
    /// <param name="request">Caller-supplied approval fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/approve")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid workflowInstanceId,
        [FromBody] ApproveStepRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApproveStepCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            Notes: request?.Notes,
            ApprovedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a human rejection decision on a WorkflowInstance's current step. Terminal.</summary>
    /// <param name="workflowInstanceId">Identity of the instance whose current step is being rejected.</param>
    /// <param name="request">Caller-supplied rejection fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/reject")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RejectAsync(
        Guid workflowInstanceId,
        [FromBody] RejectStepRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RejectStepCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            Reason: request?.Reason,
            RejectedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Advances a WorkflowInstance to the next step (or to Completed) per its definition's guarded transition graph.</summary>
    /// <param name="workflowInstanceId">Identity of the instance to advance.</param>
    /// <param name="request">Optional context updates to merge in before resolving the next transition.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/advance")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdvanceAsync(
        Guid workflowInstanceId,
        [FromBody] AdvanceWorkflowStepRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AdvanceWorkflowStepCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            ContextUpdates: request?.ContextUpdates,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Escalates a WorkflowInstance's current step to its designated escalation actor.</summary>
    /// <param name="workflowInstanceId">Identity of the instance to escalate.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/escalate")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EscalateAsync(
        Guid workflowInstanceId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EscalateWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules a bounded retry attempt of a WorkflowInstance's current step.</summary>
    /// <param name="workflowInstanceId">Identity of the instance whose current step should be retried.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/retry")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetryAsync(
        Guid workflowInstanceId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetryWorkflowStepCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Cancels a WorkflowInstance before it reaches a terminal state.</summary>
    /// <param name="workflowInstanceId">Identity of the instance to cancel.</param>
    /// <param name="request">Optional cancellation reason.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/cancel")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelAsync(
        Guid workflowInstanceId,
        [FromBody] CancelWorkflowRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CancelWorkflowCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            Reason: request?.Reason,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Creates a new, inactive (draft) WorkflowDefinition.</summary>
    /// <param name="request">Caller-supplied fields for the new definition.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("definitions")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkflowDefinitionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkflowDefinitionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDefinitionAsync(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateWorkflowDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            WorkflowTypeCode: request.WorkflowTypeCode,
            Name: request.Name,
            Description: request.Description,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adds a step to a draft WorkflowDefinition's step graph.</summary>
    /// <param name="workflowDefinitionId">Identity of the target WorkflowDefinition.</param>
    /// <param name="request">Caller-supplied step fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("definitions/{workflowDefinitionId:guid}/steps")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AddWorkflowStepDefinitionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AddWorkflowStepDefinitionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddStepAsync(
        Guid workflowDefinitionId,
        [FromBody] AddWorkflowStepDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddWorkflowStepDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowDefinitionId: workflowDefinitionId,
            Order: request.Order,
            Name: request.Name,
            StepTypeCode: request.StepTypeCode,
            AssignedRoleCode: request.AssignedRoleCode,
            AssignedUserId: request.AssignedUserId,
            EscalationRoleCode: request.EscalationRoleCode,
            EntryConditionField: request.EntryConditionField,
            EntryConditionOperatorCode: request.EntryConditionOperatorCode,
            EntryConditionExpectedValue: request.EntryConditionExpectedValue,
            TimeoutSeconds: request.TimeoutSeconds,
            TimeoutActionCode: request.TimeoutActionCode,
            RetryMaxAttempts: request.RetryMaxAttempts,
            RetryBackoffStrategyCode: request.RetryBackoffStrategyCode,
            RetryBaseDelaySeconds: request.RetryBaseDelaySeconds,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adds a directed transition between two steps of a draft WorkflowDefinition.</summary>
    /// <param name="workflowDefinitionId">Identity of the target WorkflowDefinition.</param>
    /// <param name="request">Caller-supplied transition fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("definitions/{workflowDefinitionId:guid}/transitions")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<AddWorkflowTransitionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AddWorkflowTransitionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddTransitionAsync(
        Guid workflowDefinitionId,
        [FromBody] AddWorkflowTransitionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddWorkflowTransitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowDefinitionId: workflowDefinitionId,
            FromStepId: request.FromStepId,
            ToStepId: request.ToStepId,
            GuardField: request.GuardField,
            GuardOperatorCode: request.GuardOperatorCode,
            GuardExpectedValue: request.GuardExpectedValue,
            IsDefault: request.IsDefault,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a WorkflowDefinition, making it eligible to have WorkflowInstances started against it.</summary>
    /// <param name="workflowDefinitionId">Identity of the definition to activate.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("definitions/{workflowDefinitionId:guid}/activate")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkflowDefinitionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkflowDefinitionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateDefinitionAsync(
        Guid workflowDefinitionId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateWorkflowDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowDefinitionId: workflowDefinitionId,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an automated-decision outcome on a WorkflowInstance's current step, resolved by a
    /// caller-supplied domain policy/rule without human input.</summary>
    /// <param name="workflowInstanceId">Identity of the instance whose current step the decision applies to.</param>
    /// <param name="request">Caller-supplied automated-decision fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/automated-decision")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MakeAutomatedDecisionAsync(
        Guid workflowInstanceId,
        [FromBody] MakeAutomatedDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new MakeAutomatedDecisionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            Approved: request.Approved,
            Notes: request.Notes,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Attaches a predictive recommendation to a WorkflowInstance's current PredictiveRecommendation step.</summary>
    /// <param name="workflowInstanceId">Identity of the instance whose current step the recommendation applies to.</param>
    /// <param name="request">Caller-supplied recommendation reference.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("instances/{workflowInstanceId:guid}/recommendation")]
    [Authorize(Policy = WorkflowAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SurfaceRecommendationAsync(
        Guid workflowInstanceId,
        [FromBody] SurfaceRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SurfaceRecommendationCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            WorkflowInstanceId: workflowInstanceId,
            RecommendationReference: request.RecommendationReference,
            RequestedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}

// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-02
// FILE DESCRIPTION: Typed API client for the Workflow module (Phase 5 §5 —
//   human + machine workflow runtime). Wraps HTTP communication for the
//   Application-layer Workflow queries (GetPendingApprovalsForActorQuery,
//   GetWorkflowInstanceQuery, GetWorkflowHistoryQuery,
//   GetWorkflowDefinitionByIdQuery) and lifecycle-transition commands
//   (StartWorkflowCommand, ApproveStepCommand, RejectStepCommand,
//   AdvanceWorkflowStepCommand, EscalateWorkflowCommand,
//   RetryWorkflowStepCommand, CancelWorkflowCommand). All methods return
//   ApiResponseEnvelope&lt;T&gt;. No HttpClient calls are permitted outside this
//   class for the Workflow module.
//
//   NOTE: as of this writing, PlantSafety.Api does not yet expose a V1
//   controller for the Workflow module (no
//   PlantSafety.Api/Workflow/Controllers directory exists). This client is
//   built ahead of that Api-layer work, targeting the REST surface implied
//   by the Application-layer command/query set and mirroring
//   PlantSafety.Api.Analytics.Dashboard.Controllers.V1.DashboardV1Controller's
//   command-dispatch convention (POST {resource}/{id}/{action}) plus
//   PlantSafety.UI.Modules.IncidentManagement.ApiClients.IncidentManagementApiClient's
//   graceful-degradation pattern (calls against a not-yet-implemented route
//   surface as a Problem/Fault and the UI degrades via ErrorBoundaryComponent).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Workflow.Models;

namespace PlantSafety.UI.Modules.Workflow.ApiClients;

/// <summary>Typed API client for the Workflow module.</summary>
public sealed class WorkflowApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="WorkflowApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public WorkflowApiClient(HttpClient http, ILogger<WorkflowApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves all WorkflowInstances currently pending an approval decision from the given actor.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<WorkflowInstanceDto>>> GetPendingApprovalsAsync(
        Guid userId, IReadOnlyList<string> userRoles, CancellationToken cancellationToken = default)
    {
        var query = $"v1/workflow/approvals/pending?userId={userId:D}";
        foreach (var role in userRoles)
        {
            query += $"&roles={Uri.EscapeDataString(role)}";
        }

        return GetAsync<IReadOnlyList<WorkflowInstanceDto>>(query, cancellationToken);
    }

    /// <summary>Retrieves a single WorkflowInstance read-model record by identity.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceDto>> GetInstanceAsync(
        Guid workflowInstanceId, CancellationToken cancellationToken = default)
        => GetAsync<WorkflowInstanceDto>($"v1/workflow/instances/{workflowInstanceId}", cancellationToken);

    /// <summary>Retrieves the full step execution history of a WorkflowInstance, in activation order.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<WorkflowStepHistoryEntryDto>>> GetHistoryAsync(
        Guid workflowInstanceId, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<WorkflowStepHistoryEntryDto>>($"v1/workflow/instances/{workflowInstanceId}/history", cancellationToken);

    /// <summary>Retrieves a WorkflowDefinition and its step graph by identity.</summary>
    public Task<ApiResponseEnvelope<WorkflowDefinitionDto>> GetDefinitionAsync(
        Guid workflowDefinitionId, CancellationToken cancellationToken = default)
        => GetAsync<WorkflowDefinitionDto>($"v1/workflow/definitions/{workflowDefinitionId}", cancellationToken);

    /// <summary>Starts a new WorkflowInstance from an activated WorkflowDefinition.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceStatusResult>> StartWorkflowAsync(
        StartWorkflowRequest request, CancellationToken cancellationToken = default)
        => PostAsync<StartWorkflowRequest, WorkflowInstanceStatusResult>("v1/workflow/instances/start", request, cancellationToken);

    /// <summary>Records a human approval decision on a WorkflowInstance's current step.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceStatusResult>> ApproveStepAsync(
        Guid workflowInstanceId, string? notes, CancellationToken cancellationToken = default)
        => PostAsync<ApproveStepRequest, WorkflowInstanceStatusResult>(
            $"v1/workflow/instances/{workflowInstanceId}/approve", new ApproveStepRequest(notes), cancellationToken);

    /// <summary>Records a human rejection decision on a WorkflowInstance's current step. Terminal.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceStatusResult>> RejectStepAsync(
        Guid workflowInstanceId, string? reason, CancellationToken cancellationToken = default)
        => PostAsync<RejectStepRequest, WorkflowInstanceStatusResult>(
            $"v1/workflow/instances/{workflowInstanceId}/reject", new RejectStepRequest(reason), cancellationToken);

    /// <summary>Advances a WorkflowInstance to the next step (or to Completed) per its definition's guarded transition graph.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceStatusResult>> AdvanceAsync(
        Guid workflowInstanceId, CancellationToken cancellationToken = default)
        => PostAsync<object, WorkflowInstanceStatusResult>($"v1/workflow/instances/{workflowInstanceId}/advance", new { }, cancellationToken);

    /// <summary>Escalates a WorkflowInstance's current step to its designated escalation actor.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceStatusResult>> EscalateAsync(
        Guid workflowInstanceId, CancellationToken cancellationToken = default)
        => PostAsync<object, WorkflowInstanceStatusResult>($"v1/workflow/instances/{workflowInstanceId}/escalate", new { }, cancellationToken);

    /// <summary>Schedules a bounded retry attempt of a WorkflowInstance's current step.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceStatusResult>> RetryAsync(
        Guid workflowInstanceId, CancellationToken cancellationToken = default)
        => PostAsync<object, WorkflowInstanceStatusResult>($"v1/workflow/instances/{workflowInstanceId}/retry", new { }, cancellationToken);

    /// <summary>Cancels a WorkflowInstance before it reaches a terminal state.</summary>
    public Task<ApiResponseEnvelope<WorkflowInstanceStatusResult>> CancelAsync(
        Guid workflowInstanceId, string? reason, CancellationToken cancellationToken = default)
        => PostAsync<object, WorkflowInstanceStatusResult>($"v1/workflow/instances/{workflowInstanceId}/cancel", new { reason }, cancellationToken);
}

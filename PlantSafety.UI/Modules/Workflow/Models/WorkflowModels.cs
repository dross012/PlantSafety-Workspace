// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-02
// FILE DESCRIPTION: Client-side DTOs for the Workflow module (Phase 5 §5 —
//   human + machine workflow runtime). Mirror
//   PlantSafety.Application.Workflow.Models.* and the lifecycle-transition
//   command result/request shapes field-for-field so WorkflowApiClient can
//   deserialize real backend responses directly, once the Workflow V1
//   controller is added under PlantSafety.Api/Workflow/Controllers/V1
//   (mirroring the existing DashboardV1Controller command-dispatch pattern).
//   Display-only DTOs + thin request payload records — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Workflow.Models;

/// <summary>Read-model DTO for a WorkflowInstance (mirrors <c>WorkflowInstanceDto</c>).</summary>
public sealed record WorkflowInstanceDto(
    Guid WorkflowInstanceId,
    Guid WorkflowDefinitionId,
    Guid TenantId,
    Guid? SiteId,
    string WorkflowType,
    string State,
    Guid? CurrentStepId,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    Guid InitiatedBy,
    string? ETag);

/// <summary>Read-model DTO for a single execution record within a WorkflowInstance's history (mirrors <c>WorkflowStepHistoryEntryDto</c>).</summary>
public sealed record WorkflowStepHistoryEntryDto(
    Guid StepInstanceId,
    Guid StepId,
    string StepType,
    string Status,
    string? AssignedRoleCode,
    Guid? AssignedUserId,
    DateTime? ActivatedAtUtc,
    DateTime? DecidedAtUtc,
    Guid? DecidedBy,
    string? DecisionNotes,
    int AttemptCount,
    DateTime? NextAttemptAtUtc,
    string? RecommendationReference);

/// <summary>Read-model DTO for a single WorkflowDefinition step (mirrors <c>WorkflowStepDefinitionDto</c>).</summary>
public sealed record WorkflowStepDefinitionDto(
    Guid StepId,
    int Order,
    string Name,
    string StepType,
    string? AssignedRoleCode,
    Guid? AssignedUserId);

/// <summary>Read-model DTO for a WorkflowDefinition and its step graph (mirrors <c>WorkflowDefinitionDto</c>).</summary>
public sealed record WorkflowDefinitionDto(
    Guid WorkflowDefinitionId,
    Guid TenantId,
    Guid? SiteId,
    string WorkflowType,
    string Name,
    string Description,
    int Version,
    bool IsActive,
    IReadOnlyList<WorkflowStepDefinitionDto> Steps,
    DateTime CreatedAtUtc,
    Guid CreatedBy,
    string? ETag);

/// <summary>Result payload shared by WorkflowInstance lifecycle-transition commands (mirrors <c>WorkflowInstanceStatusResult</c>).</summary>
public sealed record WorkflowInstanceStatusResult(Guid WorkflowInstanceId, string State, Guid? CurrentStepId);

/// <summary>Request body for <see cref="ApiClients.WorkflowApiClient.ApproveStepAsync"/> (mirrors <c>ApproveStepCommand</c>'s caller-supplied fields).</summary>
public sealed record ApproveStepRequest(string? Notes);

/// <summary>Request body for <see cref="ApiClients.WorkflowApiClient.RejectStepAsync"/> (mirrors <c>RejectStepCommand</c>'s caller-supplied fields).</summary>
public sealed record RejectStepRequest(string? Reason);

/// <summary>Request body for <see cref="ApiClients.WorkflowApiClient.StartWorkflowAsync"/> (mirrors <c>StartWorkflowCommand</c>'s caller-supplied fields).</summary>
public sealed record StartWorkflowRequest(Guid WorkflowDefinitionId, Guid? SiteId, IReadOnlyDictionary<string, string>? InitialContextData);

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           WorkflowInstanceDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO for a WorkflowInstance, for presentation and API responses.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Workflow.Models;

/// <summary>Read-model DTO for a WorkflowInstance.</summary>
/// <param name="WorkflowInstanceId">Instance identity.</param>
/// <param name="WorkflowDefinitionId">Identity of the originating definition.</param>
/// <param name="TenantId">Owning tenant.</param>
/// <param name="SiteId">Owning site, if site-scoped.</param>
/// <param name="WorkflowType">Workflow category code.</param>
/// <param name="State">Overall lifecycle state code.</param>
/// <param name="CurrentStepId">Identity of the currently active step, if any.</param>
/// <param name="StartedAtUtc">UTC instant the instance was started.</param>
/// <param name="CompletedAtUtc">UTC instant the instance reached a terminal state, if any.</param>
/// <param name="InitiatedBy">Identity of the user who initiated the instance.</param>
/// <param name="ETag">Optimistic-concurrency token for subsequent update commands.</param>
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

/// <summary>Read-model DTO for a single execution record within a WorkflowInstance's history.</summary>
/// <param name="StepInstanceId">Step-instance identity.</param>
/// <param name="StepId">Identity of the definition step this record executed.</param>
/// <param name="StepType">Step resolution mode code.</param>
/// <param name="Status">Runtime status code.</param>
/// <param name="AssignedRoleCode">Role code assigned to resolve this step, if role-assigned.</param>
/// <param name="AssignedUserId">Specific user assigned to resolve this step, if user-assigned.</param>
/// <param name="ActivatedAtUtc">UTC instant this step became active, if activated.</param>
/// <param name="DecidedAtUtc">UTC instant a decision was recorded, if any.</param>
/// <param name="DecidedBy">Identity of the user who recorded the decision, if a human decision was made.</param>
/// <param name="DecisionNotes">Free-text notes captured with the decision, if any.</param>
/// <param name="AttemptCount">Number of retry attempts made so far.</param>
/// <param name="NextAttemptAtUtc">UTC instant the next retry attempt is scheduled for, if a retry is pending.</param>
/// <param name="RecommendationReference">Attached predictive recommendation reference, if any.</param>
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

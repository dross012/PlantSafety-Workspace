// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           WorkflowDefinitionDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO for a WorkflowDefinition, including its full step graph, for
//                  presentation and API responses.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Workflow.Models;

/// <summary>Read-model DTO for a single WorkflowDefinition step.</summary>
/// <param name="StepId">Step identity.</param>
/// <param name="Order">1-based ordinal position within the graph.</param>
/// <param name="Name">Human-readable step name.</param>
/// <param name="StepType">Step resolution mode code.</param>
/// <param name="AssignedRoleCode">Role code assigned to resolve this step, if role-assigned.</param>
/// <param name="AssignedUserId">Specific user assigned to resolve this step, if user-assigned.</param>
public sealed record WorkflowStepDefinitionDto(
    Guid StepId,
    int Order,
    string Name,
    string StepType,
    string? AssignedRoleCode,
    Guid? AssignedUserId);

/// <summary>Read-model DTO for a WorkflowDefinition and its step graph.</summary>
/// <param name="WorkflowDefinitionId">Definition identity.</param>
/// <param name="TenantId">Owning tenant.</param>
/// <param name="SiteId">Owning site, if site-scoped.</param>
/// <param name="WorkflowType">Workflow category code.</param>
/// <param name="Name">Human-readable name.</param>
/// <param name="Description">Free-form description.</param>
/// <param name="Version">Definition revision number.</param>
/// <param name="IsActive">Whether the definition is active.</param>
/// <param name="Steps">The definition's step graph nodes, in order.</param>
/// <param name="CreatedAtUtc">UTC creation timestamp.</param>
/// <param name="CreatedBy">Identity of the user who created this record.</param>
/// <param name="ETag">Optimistic-concurrency token for subsequent update commands.</param>
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

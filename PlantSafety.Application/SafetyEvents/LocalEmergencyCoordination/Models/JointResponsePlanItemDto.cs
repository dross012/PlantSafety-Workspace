// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           JointResponsePlanItemDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model item DTO for a single joint response plan, backing the Active Joint
//                  Response Status dashboard and GetJointResponsePlanByIncidentQuery.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

/// <summary>A single joint response plan tracked by the LocalEmergencyCoordination module.</summary>
public sealed record JointResponsePlanItemDto
{
    /// <summary>Plan identifier.</summary>
    public required Guid JointResponsePlanId { get; init; }

    /// <summary>Identity of the parent LocalEmergencyCoordination case.</summary>
    public required Guid LocalEmergencyCoordinationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Identity of the incident this plan supports, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Descriptive plan name.</summary>
    public required string PlanName { get; init; }

    /// <summary>Whether the plan is currently active.</summary>
    public bool IsActive { get; init; }

    /// <summary>UTC instant the plan was activated.</summary>
    public required DateTime ActivatedAtUtc { get; init; }

    /// <summary>Identity of the user who activated the plan.</summary>
    public required Guid ActivatedBy { get; init; }

    /// <summary>UTC instant the plan was deactivated, if any.</summary>
    public DateTime? DeactivatedAtUtc { get; init; }
}

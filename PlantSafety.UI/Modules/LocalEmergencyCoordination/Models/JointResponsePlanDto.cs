// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTO for a joint response plan. Mirrors
//   PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models.JointResponsePlanItemDto.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.LocalEmergencyCoordination.Models;

/// <summary>Read-side projection of a joint response plan coordinated with external local emergency responders.</summary>
public sealed class JointResponsePlanDto
{
    /// <summary>Gets the plan identifier.</summary>
    public Guid JointResponsePlanId { get; init; }

    /// <summary>Gets the identity of the incident this plan supports, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Gets the descriptive plan name.</summary>
    public string PlanName { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the plan is currently active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the UTC instant the plan was activated.</summary>
    public DateTimeOffset ActivatedAtUtc { get; init; }
}

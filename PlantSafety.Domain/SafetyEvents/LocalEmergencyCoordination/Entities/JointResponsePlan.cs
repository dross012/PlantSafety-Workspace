// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           JointResponsePlan.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity representing a joint response plan activated in coordination
//                  with one or more external local emergency responders for a specific incident.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Entities;

/// <summary>A joint response plan coordinated with external local emergency responders.</summary>
public sealed class JointResponsePlan
{
    /// <summary>Unique identity of this joint response plan.</summary>
    public JointResponsePlanId JointResponsePlanId { get; private set; }

    /// <summary>Identity of the parent LocalEmergencyCoordination aggregate.</summary>
    public LocalEmergencyCoordinationId LocalEmergencyCoordinationId { get; private set; }

    /// <summary>Identity of the SafetyEvents incident this plan supports, if any (correlates with EmergencyManagement/CrisisManagement).</summary>
    public Guid? IncidentId { get; private set; }

    /// <summary>Descriptive plan name.</summary>
    public string PlanName { get; private set; } = string.Empty;

    /// <summary>Indicates whether the plan is currently active.</summary>
    public bool IsActive { get; private set; }

    /// <summary>UTC instant the plan was activated.</summary>
    public UtcTimestamp ActivatedAtUtc { get; private set; }

    /// <summary>Identity of the user who activated the plan.</summary>
    public UserId ActivatedBy { get; private set; }

    /// <summary>UTC instant the plan was deactivated, or null while still active.</summary>
    public UtcTimestamp? DeactivatedAtUtc { get; private set; }

    /// <summary>Identity of the user who deactivated the plan, or null while still active.</summary>
    public UserId? DeactivatedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private JointResponsePlan() { }

    /// <summary>Creates and activates a new JointResponsePlan child entity.</summary>
    internal static JointResponsePlan Activate(
        LocalEmergencyCoordinationId localEmergencyCoordinationId,
        Guid? incidentId,
        string planName,
        UtcTimestamp activatedAtUtc,
        UserId activatedBy)
        => new()
        {
            JointResponsePlanId          = JointResponsePlanId.New(),
            LocalEmergencyCoordinationId = localEmergencyCoordinationId,
            IncidentId                   = incidentId,
            PlanName                     = (planName ?? string.Empty).Trim(),
            IsActive                     = true,
            ActivatedAtUtc               = activatedAtUtc,
            ActivatedBy                  = activatedBy,
        };

    /// <summary>Deactivates a previously activated joint response plan.</summary>
    internal void Deactivate(UtcTimestamp deactivatedAtUtc, UserId deactivatedBy)
    {
        IsActive         = false;
        DeactivatedAtUtc = deactivatedAtUtc;
        DeactivatedBy    = deactivatedBy;
    }
}

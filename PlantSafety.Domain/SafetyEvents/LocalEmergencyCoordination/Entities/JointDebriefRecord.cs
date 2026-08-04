// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           JointDebriefRecord.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity capturing a post-incident debrief conducted jointly with external
//                  local emergency responder agencies. Backs the "joint debrief completion rate"
//                  KPI and the Post-Incident Debrief Summary dashboard.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Entities;

/// <summary>A post-incident debrief record conducted jointly with external responder agencies.</summary>
public sealed class JointDebriefRecord
{
    /// <summary>Unique identity of this debrief record.</summary>
    public JointDebriefRecordId JointDebriefRecordId { get; private set; }

    /// <summary>Identity of the parent LocalEmergencyCoordination aggregate.</summary>
    public LocalEmergencyCoordinationId LocalEmergencyCoordinationId { get; private set; }

    /// <summary>Identity of the SafetyEvents incident this debrief covers, if any.</summary>
    public Guid? IncidentId { get; private set; }

    /// <summary>Comma-delimited or free-text list of external agencies that participated.</summary>
    public string AgenciesInvolved { get; private set; } = string.Empty;

    /// <summary>Debrief summary/findings.</summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>UTC instant the debrief was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>Identity of the user who recorded the debrief.</summary>
    public UserId RecordedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private JointDebriefRecord() { }

    /// <summary>Creates a new joint post-incident debrief record.</summary>
    internal static JointDebriefRecord Create(
        LocalEmergencyCoordinationId localEmergencyCoordinationId,
        Guid? incidentId,
        string agenciesInvolved,
        string summary,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy)
        => new()
        {
            JointDebriefRecordId         = JointDebriefRecordId.New(),
            LocalEmergencyCoordinationId = localEmergencyCoordinationId,
            IncidentId                   = incidentId,
            AgenciesInvolved             = (agenciesInvolved ?? string.Empty).Trim(),
            Summary                      = (summary ?? string.Empty).Trim(),
            RecordedAtUtc                = recordedAtUtc,
            RecordedBy                   = recordedBy,
        };
}

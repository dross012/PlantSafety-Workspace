// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTO for a joint post-incident debrief record. Mirrors
//   PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models.JointDebriefItemDto.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.LocalEmergencyCoordination.Models;

/// <summary>Read-side projection of a joint post-incident debrief record.</summary>
public sealed class JointDebriefDto
{
    /// <summary>Gets the debrief record identifier.</summary>
    public Guid JointDebriefRecordId { get; init; }

    /// <summary>Gets the identity of the incident this debrief covers, if any.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Gets the agencies that participated in the debrief.</summary>
    public string AgenciesInvolved { get; init; } = string.Empty;

    /// <summary>Gets the debrief summary/findings.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the debrief was recorded.</summary>
    public DateTimeOffset RecordedAtUtc { get; init; }
}

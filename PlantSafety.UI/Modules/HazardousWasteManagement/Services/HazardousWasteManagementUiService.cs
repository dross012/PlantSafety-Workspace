// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Presentation-mapping service for the HazardousWasteManagement module. Maps
//   raw API DTO fields to the display vocabulary the canonical Shared components expect
//   (StatusIndicatorComponent's Healthy/Warning/Critical/Offline states). Pure/stateless — no
//   HTTP, no state mutation.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HazardousWasteManagement.Services;

/// <summary>
/// Stateless mapping helpers that translate HazardousWasteManagement DTO fields into the
/// vocabulary expected by the canonical Shared UI components.
/// </summary>
public static class HazardousWasteManagementUiService
{
    /// <summary>Maps a manifest's free-text lifecycle status string to the closest operational state.</summary>
    /// <param name="status">The raw status string from <see cref="Models.HazardousWasteManifestDto"/>.</param>
    public static string MapManifestStatusToOperationalState(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "closed" => "Healthy",
            "generated" or "intransit" or "in-transit" or "receivedattsdf" or "received-at-tsdf" => "Warning",
            "discrepancyflagged" or "discrepancy-flagged" => "Critical",
            "" => "Offline",
            _ => "Offline",
        };

    /// <summary>Maps an accumulation area's free-text lifecycle status string to the closest operational state.</summary>
    /// <param name="status">The raw status string from <see cref="Models.AccumulationAreaDto"/>.</param>
    public static string MapAccumulationAreaStatusToOperationalState(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "active" => "Healthy",
            "timelimitwarning" or "time-limit-warning" => "Warning",
            "inactive" => "Offline",
            "" => "Offline",
            _ => "Offline",
        };

    /// <summary>Returns <see langword="true"/> when a container deadline has already passed (overdue, must escalate to RiskManagement).</summary>
    /// <param name="deadlineUtc">The earliest container deadline for the area.</param>
    public static bool IsOverdue(DateTime? deadlineUtc) => deadlineUtc.HasValue && deadlineUtc.Value < DateTime.UtcNow;
}

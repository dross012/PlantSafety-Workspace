// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Presentation-mapping service for the Equipment Safety
//   module. Maps raw API DTO fields to the display vocabulary the canonical
//   Shared components expect (StatusIndicatorComponent's Healthy/Warning/
//   Critical/Offline states). Pure/stateless — no HTTP, no state mutation.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.EquipmentSafety.Services;

/// <summary>
/// Stateless mapping helpers that translate Equipment Safety DTO fields into the
/// vocabulary expected by the canonical Shared UI components.
/// </summary>
public static class EquipmentSafetyUiService
{
    /// <summary>
    /// Maps a free-text lifecycle status string to the closest
    /// <c>StatusIndicatorComponent</c> operational state.
    /// </summary>
    /// <param name="status">The raw status string from <see cref="Models.EquipmentSafetyDto"/>.</param>
    /// <returns>One of Healthy, Warning, Critical, or Offline.</returns>
    public static string MapStatusToOperationalState(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "closed" or "complete" or "completed" or "passed" or "approved" or "resolved" or "compliant" or "safe" or "operational" => "Healthy",
            "open" or "pending" or "in-progress" or "inprogress" or "issued" or "active" or "scheduled" or "in review" => "Warning",
            "failed" or "rejected" or "overdue" or "expired" or "unsafe" or "critical" or "non-compliant" or "noncompliant" or "out of service" => "Critical",
            "" => "Offline",
            _ => "Offline",
        };
}

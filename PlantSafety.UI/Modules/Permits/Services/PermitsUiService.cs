// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Presentation-mapping service for the Permits to Work
//   module. Maps raw API DTO fields to the display vocabulary the canonical
//   Shared components expect (StatusIndicatorComponent's Healthy/Warning/
//   Critical/Offline states). Pure/stateless — no HTTP, no state mutation.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Permits.Services;

/// <summary>
/// Stateless mapping helpers that translate Permits to Work DTO fields into the
/// vocabulary expected by the canonical Shared UI components.
/// </summary>
public static class PermitsUiService
{
    /// <summary>
    /// Maps a free-text lifecycle status string to the closest
    /// <c>StatusIndicatorComponent</c> operational state.
    /// </summary>
    /// <param name="status">The raw status string from <see cref="Models.PermitDto"/>.</param>
    /// <returns>One of Healthy, Warning, Critical, or Offline.</returns>
    public static string MapStatusToOperationalState(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "closed" or "complete" or "completed" or "approved" or "compliant" => "Healthy",
            "open" or "pending" or "issued" or "active" or "in-progress" or "inprogress" or "in review" => "Warning",
            "expired" or "rejected" or "revoked" or "overdue" or "suspended" => "Critical",
            "" => "Offline",
            _ => "Offline",
        };
}

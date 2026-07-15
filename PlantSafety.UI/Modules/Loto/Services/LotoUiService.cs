// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Presentation-mapping service for the LOTO module. Maps
//   raw API DTO fields to the display vocabulary the canonical Shared
//   components expect (StatusIndicatorComponent's Healthy/Warning/Critical/
//   Offline states). Pure/stateless — no HTTP, no state mutation.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Loto.Services;

/// <summary>
/// Stateless mapping helpers that translate LOTO DTO fields into the vocabulary
/// expected by the canonical Shared UI components.
/// </summary>
public static class LotoUiService
{
    /// <summary>
    /// Maps a free-text lifecycle status string to the closest
    /// <c>StatusIndicatorComponent</c> operational state. For LOTO, an active
    /// lock is intentionally treated as a Warning (equipment is de-energized and
    /// off-limits, not a failure state) — Critical is reserved for overdue or
    /// improperly released locks.
    /// </summary>
    /// <param name="status">The raw status string from <see cref="Models.LotoDto"/>.</param>
    /// <returns>One of Healthy, Warning, Critical, or Offline.</returns>
    public static string MapStatusToOperationalState(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "released" or "removed" or "closed" or "complete" or "completed" or "verified" => "Healthy",
            "locked" or "active" or "applied" or "pending" or "in-progress" or "inprogress" => "Warning",
            "overdue" or "expired" or "improperly released" or "violation" or "bypassed" => "Critical",
            "" => "Offline",
            _ => "Offline",
        };
}

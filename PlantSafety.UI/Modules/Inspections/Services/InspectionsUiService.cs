// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Presentation-mapping service for the Inspections module.
//   Maps raw API DTO fields to the display vocabulary the canonical Shared
//   components expect (StatusIndicatorComponent's Healthy/Warning/Critical/
//   Offline states, applied to both lifecycle Status and inspection
//   Outcome). Pure/stateless — no HTTP, no state mutation.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Inspections.Services;

/// <summary>
/// Stateless mapping helpers that translate Inspections DTO fields into the
/// vocabulary expected by the canonical Shared UI components.
/// </summary>
public static class InspectionsUiService
{
    /// <summary>
    /// Maps a free-text lifecycle status string to the closest
    /// <c>StatusIndicatorComponent</c> operational state.
    /// </summary>
    /// <param name="status">The raw status string from <see cref="Models.InspectionDto"/>.</param>
    /// <returns>One of Healthy, Warning, Critical, or Offline.</returns>
    public static string MapStatusToOperationalState(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "closed" or "complete" or "completed" or "approved" or "compliant" => "Healthy",
            "open" or "pending" or "in-progress" or "inprogress" or "active" or "scheduled" or "in review" => "Warning",
            "overdue" or "expired" or "rejected" or "cancelled" or "canceled" => "Critical",
            "" => "Offline",
            _ => "Offline",
        };

    /// <summary>
    /// Maps an inspection outcome string (Pass/Fail/Conditional) to the closest
    /// <c>StatusIndicatorComponent</c> operational state.
    /// </summary>
    /// <param name="outcome">The raw outcome string from <see cref="Models.InspectionDto"/>.</param>
    /// <returns>One of Healthy, Warning, Critical, or Offline.</returns>
    public static string MapOutcomeToOperationalState(string outcome) =>
        outcome.Trim().ToLowerInvariant() switch
        {
            "pass" or "passed" or "compliant" => "Healthy",
            "conditional" or "conditional pass" or "partial" => "Warning",
            "fail" or "failed" or "non-compliant" or "noncompliant" => "Critical",
            "" => "Offline",
            _ => "Offline",
        };
}

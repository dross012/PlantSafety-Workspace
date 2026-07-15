// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Presentation-mapping service for the Hazard Assessment
//   module. Maps raw API DTO fields to the display vocabulary the canonical
//   Shared components expect (StatusIndicatorComponent's Healthy/Warning/
//   Critical/Offline states; SeverityBadgeComponent's Critical/High/Medium/
//   Low/Info scale). Pure/stateless — no HTTP, no state mutation.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HazardAssessment.Services;

/// <summary>
/// Stateless mapping helpers that translate Hazard Assessment DTO fields into the
/// vocabulary expected by the canonical Shared UI components.
/// </summary>
public static class HazardAssessmentUiService
{
    /// <summary>
    /// Maps a free-text lifecycle status string to the closest
    /// <c>StatusIndicatorComponent</c> operational state.
    /// </summary>
    /// <param name="status">The raw status string from <see cref="Models.HazardAssessmentDto"/>.</param>
    /// <returns>One of Healthy, Warning, Critical, or Offline.</returns>
    public static string MapStatusToOperationalState(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "closed" or "complete" or "completed" or "resolved" or "mitigated" or "approved" or "compliant" => "Healthy",
            "open" or "pending" or "in-progress" or "inprogress" or "active" or "scheduled" or "in review" => "Warning",
            "overdue" or "expired" or "rejected" or "escalated" or "unresolved" => "Critical",
            "" => "Offline",
            _ => "Offline",
        };

    /// <summary>
    /// Normalizes a risk-level string for <c>SeverityBadgeComponent</c>. The component already
    /// handles Critical/High/Medium/Low/Info case-insensitively; unrecognized values fall through
    /// to its neutral badge style, so this only trims incoming whitespace.
    /// </summary>
    /// <param name="riskLevel">The raw risk level string from <see cref="Models.HazardAssessmentDto"/>.</param>
    /// <returns>The trimmed risk level, ready to pass directly to <c>SeverityBadgeComponent.Severity</c>.</returns>
    public static string NormalizeSeverity(string riskLevel) => riskLevel.Trim();
}

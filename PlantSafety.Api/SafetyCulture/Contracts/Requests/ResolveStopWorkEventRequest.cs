// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for resolving a StopWorkEvent over HTTP.

namespace PlantSafety.Api.SafetyCulture.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/safetyculture/stop-work-events/{stopWorkEventId}/resolve</c>.</summary>
/// <param name="ResolutionSummary">Narrative summary of the resolution.</param>
/// <param name="WasCelebrated">Whether the stop-work action was formally celebrated/recognized.</param>
public sealed record ResolveStopWorkEventRequest(
    string ResolutionSummary,
    bool WasCelebrated);
